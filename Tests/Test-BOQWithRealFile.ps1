<#
.SYNOPSIS
    Tests the BOQ Activity Generator with a real BOQ Excel file
#>
param(
    [string]$ExcelPath = "C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
    [string]$TestFile = "$PSScriptRoot\Book2.xlsx",
    [switch]$NoCleanup
)

# Kill existing Excel
Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

Write-Host "Opening: $TestFile"
$proc = Start-Process -FilePath $ExcelPath -ArgumentList "`"$TestFile`"" -PassThru
Start-Sleep -Seconds 10

try {
    $excel = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application")
    $excel.Visible = $true
    $excel.DisplayAlerts = $false
    Write-Host "[PASS] Excel launched (PID: $($proc.Id))"

    $addin = $excel.COMAddIns | Where-Object { $_.Description -like "*Som3a Addin 2026*" }
    if (-not $addin) { throw "Add-in not found" }
    Write-Host "[PASS] Add-in loaded: $($addin.ProgId)"

    Start-Sleep -Seconds 3
    $auto = $addin.Object
    $status = $auto.GetStatus()
    Write-Host "[PASS] COM automation: $status"

    # Workbook info
    if ($excel.ActiveWorkbook) {
        Write-Host "[INFO] Workbook: $($excel.ActiveWorkbook.Name)"
        Write-Host "[INFO] Sheets: $($excel.ActiveWorkbook.Worksheets.Count)"
        for ($i = 1; $i -le $excel.ActiveWorkbook.Worksheets.Count; $i++) {
            $ws = $excel.ActiveWorkbook.Worksheets($i)
            Write-Host ("       [$i] " + $ws.Name)
        }
    }

    # Navigate to BOQ Activity Generator
    $result = $auto.OpenWindow("BOQ Activity Generator")
    if ($result -ne "OK") { throw "Navigation failed: $result" }
    Write-Host "[PASS] BOQ Activity Generator page opened"
    Start-Sleep -Seconds 3

    # Step 1: Configure AI to use local Ollama
    $configResult = $auto.BoqConfigureOllama("llama3.2", "http://localhost:11434")
    Write-Host "[INFO] BoqConfigureOllama: $configResult"
    if ($configResult -like "ERROR*") { throw "Ollama config failed: $configResult" }
    Write-Host "[PASS] AI provider set to Ollama (deepseek-coder)"

    # Step 2: Load BOQ from active Excel sheet
    $loadResult = $auto.BoqLoad()
    Write-Host "[INFO] BoqLoad: $loadResult"
    if ($loadResult -like "ERROR*") { throw "BOQ load failed: $loadResult" }
    if ($loadResult -match "BoqItems:(\d+)") {
        $boqItems = $Matches[1]
        if ($boqItems -eq "0") { throw "No BOQ items found in active sheet" }
    }
    Write-Host "[PASS] BOQ data loaded"

    # Step 2: Consent to AI generation
    $consentResult = $auto.BoqConsent()
    Write-Host "[INFO] BoqConsent: $consentResult"
    if ($consentResult -like "ERROR*") { throw "Consent failed: $consentResult" }
    Write-Host "[PASS] AI consent given"

    # Step 3: Generate activities via AI
    Write-Host "[INFO] Generating activities (this may take 30-60s)..."
    $genResult = $auto.BoqGenerate()
    Write-Host "[INFO] BoqGenerate: $genResult"
    if ($genResult -like "ERROR*") { throw "Generation failed: $genResult" }
    Write-Host "[PASS] Activities generated"

    # Step 4: Verify generation results
    $statusResult = $auto.BoqGetStatus()
    Write-Host "[INFO] BoqGetStatus: $statusResult"
    if ($statusResult -match "Activities:(\d+)") {
        $activityCount = $Matches[1]
        if ($activityCount -eq "0") { throw "No activities were generated" }
        Write-Host "[PASS] $activityCount activities generated successfully"
    }

    Write-Host ""
    Write-Host "===== BOQ VSTO TEST WITH Book2.xlsx PASSED ====="
    Write-Host ""

    if (-not $NoCleanup) {
        Write-Host "Press Enter to close..."
        Read-Host
    }
}
catch {
    Write-Host "[FAIL] $_" -ForegroundColor Red
    Write-Host "===== BOQ VSTO TEST FAILED ====="
    exit 1
}
finally {
    if (-not $NoCleanup) {
        try { $excel.Quit() } catch {}
        try { [Runtime.InteropServices.Marshal]::ReleaseComObject($excel) } catch {}
        Start-Sleep -Seconds 2
        Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
        Write-Host "Cleaned up."
    }
}
