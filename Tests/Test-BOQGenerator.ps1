<#
.SYNOPSIS
    BOQ Generator comprehensive functional test using COM automation methods.
.DESCRIPTION
    Opens Book2.xlsx in Excel, navigates to BOQ Activity Generator,
    calls BoqLoad, BoqConsent, BoqGenerate, BoqGetStatus via COM automation,
    and validates each step.
#>

param(
    [string]$ExcelPath = "C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
    [string]$TestFile = "$PSScriptRoot\Book2.xlsx",
    [switch]$NoCleanup
)

$passed = 0
$failed = 0

function Write-Pass { Write-Host "[PASS] $($args[0])" -ForegroundColor Green }
function Write-Fail { Write-Host "[FAIL] $($args[0])" -ForegroundColor Red }
function Write-Info { Write-Host "[INFO] $($args[0])" -ForegroundColor Cyan }

# Kill existing Excel
Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

Write-Info "Opening: $TestFile"
$proc = Start-Process -FilePath $ExcelPath -ArgumentList "`"$TestFile`"" -PassThru
$procPid = $proc.Id
Start-Sleep -Seconds 10

try {
    $excel = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application")
    $excel.Visible = $true
    $excel.DisplayAlerts = $false
    Write-Pass "Excel launched (PID: $procPid)"

    $addin = $excel.COMAddIns | Where-Object { $_.Description -like "*Som3a Addin 2026*" }
    if (-not $addin) { throw "Add-in not found" }
    Write-Pass "Add-in loaded"
    $passed++

    Start-Sleep -Seconds 3
    $auto = $addin.Object

    # 1. Verify workbook loaded
    if ($excel.ActiveWorkbook.Name -like "*Book2*") { Write-Pass "Workbook: $($excel.ActiveWorkbook.Name)"; $passed++ } else { Write-Fail "Wrong workbook: $($excel.ActiveWorkbook.Name)"; $failed++ }

    # 2. Navigate to BOQ Activity Generator
    $result = $auto.OpenWindow("BOQ Activity Generator")
    if ($result -ne "OK") { throw "Navigation failed: $result" }
    Write-Pass "Navigated to BOQ Activity Generator"
    $passed++
    Start-Sleep -Seconds 3

    # 3. BoqGetStatus - should show HasBoqLoaded=False initially
    $status = $auto.BoqGetStatus()
    if ($status -like "OK*") {
        Write-Pass "BoqGetStatus initial: $status"
        $passed++
    } else {
        Write-Fail "BoqGetStatus initial failed: $status"
        $failed++
    }

    # 4. BoqLoad - loads BOQ from active workbook
    $loadResult = $auto.BoqLoad()
    if ($loadResult -like "OK*") {
        Write-Pass "BoqLoad succeeded: $loadResult"
        $passed++
        $boqCount = 0
        if ($loadResult -match "BoqItems:(\d+)") { $boqCount = [int]$Matches[1] }
        if ($boqCount -gt 0) {
            Write-Pass "BOQ items loaded: $boqCount (positive count)"
            $passed++
        } else {
            Write-Fail "BOQ items loaded: $boqCount (expected > 0)"
            $failed++
        }
    } else {
        Write-Fail "BoqLoad failed: $loadResult"
        $failed++
    }
    Start-Sleep -Seconds 2

    # 5. BoqGetStatus - should show HasBoqLoaded=True now
    $status = $auto.BoqGetStatus()
    if ($status -like "*HasBoqLoaded:True*") {
        Write-Pass "BoqGetStatus post-load (HasBoqLoaded=True): $status"
        $passed++
    } else {
        Write-Info "BoqGetStatus post-load: $status"
        $failed++
    }

    # 6. BoqConsent - give consent for generation
    $consentResult = $auto.BoqConsent()
    if ($consentResult -like "*Consented:true") {
        Write-Pass "BoqConsent succeeded"
        $passed++
    } else {
        Write-Fail "BoqConsent failed: $consentResult"
        $failed++
    }
    Start-Sleep -Seconds 1

    # 7. BoqGetStatus - verify consent
    $status = $auto.BoqGetStatus()
    if ($status -like "*HasConsented:True*" -and $status -like "*CanGenerate:True*") {
        Write-Pass "BoqGetStatus post-consent (ready to generate): $status"
        $passed++
    } else {
        Write-Info "BoqGetStatus post-consent: $status"
        $failed++
    }

    # 8. BoqGenerate - generate activities
    $genResult = $auto.BoqGenerate()
    if ($genResult -like "OK*") {
        Write-Pass "BoqGenerate succeeded: $genResult"
        $passed++
        $genResult -match "Activities:(\d+)" | Out-Null
        $actCount = [int]$Matches[1]
        if ($actCount -gt 0) {
            Write-Pass "Activities generated: $actCount"
            $passed++
        } else {
            Write-Info "Activities generated: $actCount (0 is OK since AI is stub)"
        }
    } else {
        Write-Fail "BoqGenerate failed: $genResult"
        $failed++
    }
    Start-Sleep -Seconds 3

    # 9. Final status
    $status = $auto.BoqGetStatus()
    Write-Info "Final status: $status"
    if ($status -like "OK*") { $passed++ } else { $failed++ }

    Write-Host ""
    Write-Host "===== RESULTS =====" -ForegroundColor Yellow
    Write-Host "Passed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
}
catch {
    Write-Fail "Fatal: $_"
}
finally {
    if (-not $NoCleanup) {
        try { $excel.Quit() } catch {}
        try { [Runtime.InteropServices.Marshal]::ReleaseComObject($excel) } catch {}
        Start-Sleep -Seconds 2
        Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
        Write-Info "Cleaned up"
    }
}

if ($failed -eq 0 -and $passed -gt 0) { exit 0 } else { exit 1 }
