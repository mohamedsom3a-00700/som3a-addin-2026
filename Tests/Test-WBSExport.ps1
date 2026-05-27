param(
    [switch]$NoCleanup
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function Write-Log { param([string]$Msg) Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Msg" }

function Kill-Excel {
    $procs = Get-Process -Name "EXCEL" -ErrorAction SilentlyContinue
    if ($procs) { $procs | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue } }
    Start-Sleep -Seconds 2
}

function Launch-Excel {
    $path = "${env:ProgramFiles}\Microsoft Office\root\Office16\EXCEL.EXE"
    if (!(Test-Path $path)) { $path = "${env:ProgramFiles(x86)}\Microsoft Office\root\Office16\EXCEL.EXE" }
    if (!(Test-Path $path)) { throw "Excel not found at either path" }
    Write-Log "  Starting Excel from: $path"
    $proc = Start-Process -FilePath $path -PassThru
    Start-Sleep -Seconds 8
    $excel = $null
    for ($i = 0; $i -lt 10 -and -not $excel; $i++) {
        try { $excel = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application") } catch {}
        if (-not $excel) { Start-Sleep -Seconds 2 }
    }
    if (-not $excel) { throw "Could not get Excel Application after 28 seconds" }
    $excel.Visible = $true
    @{ Process = $proc; App = $excel }
}

function Get-AddIn {
    param($ExcelApp)
    $addins = $ExcelApp.COMAddIns
    Write-Log "  COMAddIns count: $($addins.Count)"
    foreach ($a in $addins) { 
        $desc = try { $a.Description } catch { "??" }
        $prog = try { $a.ProgId } catch { "??" }
        Write-Log "    AddIn: ProgId=$prog Description=$desc Connect=$($a.Connect)"
        if ($desc -like "*Som3a*" -or $prog -like "*Som3a*") { return $a }
    }
    return $null
}

$pass = 0
$fail = 0

try {
    # 1. Launch Excel
    Write-Log "Launching Excel..."
    Kill-Excel
    $ctx = Launch-Excel
    $excelApp = $ctx.App
    $excelApp.Workbooks.Add() | Out-Null
    Write-Log "Excel ready (PID: $($ctx.Process.Id))"

    # 2. Verify add-in loaded
    $addin = Get-AddIn -ExcelApp $excelApp
    if (-not $addin) {
        Write-Log "  COMAddIns enumeration:"
        foreach ($a in $excelApp.COMAddIns) {
            Write-Log "    ProgId=$($a.ProgId) Description=$($a.Description) Guid=$($a.Guid) Connect=$($a.Connect)"
        }
        throw "Add-in not found"
    }
    Write-Log "  Found AddIn: ProgId=$($addin.ProgId) Connect=$($addin.Connect)"
    $auto = $addin.Object
    if (-not $auto) {
        Write-Log "  [WARN] COMAddIn.Object is null, trying direct ProgId creation..."
        try { $auto = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Som3a_Addin_2026.AddInAutomation") } catch {}
        if (-not $auto) { throw "Cannot get automation object" }
    }
    $status = $auto.GetStatus()
    Write-Log "Add-in: $status"

    # 3. Navigate to WBS Template Browser
    Write-Log "Navigating to WBS Template Browser..."
    $result = $auto.OpenWindow("WBS Template Browser")
    if ($result -ne "OK") { throw "Navigation failed: $result" }
    Write-Log "  [PASS] Navigation: $result"
    $pass++
    Start-Sleep -Seconds 1

    # 4. Use COM automation to export the template directly
    Write-Log "  Calling ExportWBSTemplate via COM..."
    $result = $auto.ExportWBSTemplate("Default Construction WBS")
    if ($result -like "OK*") {
        Write-Log "  [PASS] $result"
        $pass++
        Start-Sleep -Seconds 3
    } else {
        Write-Log "  [FAIL] Export via COM failed: $result"
        $fail++
    }

    # 7. Verify a new workbook was created by checking workbook count
    try {
        $workbookCount = $excelApp.Workbooks.Count
        if ($workbookCount -ge 2) {
            Write-Log "  [PASS] New workbook created (Total: $workbookCount workbooks)"
            $pass++

            # 8. Check the exported data in the last workbook
            $wb = $excelApp.Workbooks.Item($workbookCount)
            $sh = $wb.Sheets(1)
            $cell = $sh.Cells.Item(1,1)
            $header1 = try { $cell.Text } catch { $null }
            if (-not $header1) { $header1 = try { $cell.Value2 } catch { $null } }
            if (-not $header1) { $header1 = "" }
            $header1 = $header1.Trim()
            if ($header1 -eq "Code") {
                Write-Log "  [PASS] Sheet has correct headers: '$header1'"
                $pass++

                # Check data rows exist
                $cell2 = $sh.Cells.Item(2,2)
                $row2val = try { $cell2.Text } catch { $null }
                if (-not $row2val) { $row2val = try { $cell2.Value2 } catch { $null } }
                if (-not $row2val) { $row2val = "" }
                $row2val = $row2val.Trim()
                if ($row2val -ne "") {
                    Write-Log "  [PASS] Data found in row 2: '$row2val'"
                    $pass++
                } else {
                    Write-Log "  [FAIL] No data in row 2"
                    $fail++
                }
            } else {
                Write-Log "  [FAIL] Header mismatch: '$header1'"
                $fail++
            }

            # Cleanup created workbook
            if (-not $NoCleanup) { $wb.Close($false) }
        } else {
            Write-Log "  [FAIL] No new workbook created (Count: $workbookCount)"
            $fail++
        }
    } catch {
        Write-Log "  [FAIL] Workbook verification error: $_"
        $fail++
    }

    # 9. Test import from Excel to WBS template
    try {
        Write-Log "  Calling ImportWBSTemplateFromExcel via COM..."
        $importResult = $auto.ImportWBSTemplateFromExcel("Custom")
        if ($importResult -like "OK*") {
            Write-Log "  [PASS] $importResult"
            $pass++
        } else {
            Write-Log "  [FAIL] Import via COM failed: $importResult"
            $fail++
        }
    } catch {
        Write-Log "  [FAIL] Import error: $_"
        $fail++
    }

    # Summary
    Write-Log "`n===== RESULTS ====="
    Write-Log "Passed: $pass | Failed: $fail"

} catch {
    Write-Log "FATAL: $_" -Level "ERROR"
} finally {
    if (-not $NoCleanup) {
        try { $excelApp.DisplayAlerts = $false; $excelApp.Quit() } catch {}
        try { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excelApp) | Out-Null } catch {}
        Kill-Excel
    }
}

if ($fail -gt 0) { exit 1 } else { exit 0 }
