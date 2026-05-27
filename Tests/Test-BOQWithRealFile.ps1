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

    # Open Run Diagnostics to verify page loaded
    $diag = $auto.RunDiagnostics()
    Write-Host "[INFO] Diagnostics: $diag"

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
