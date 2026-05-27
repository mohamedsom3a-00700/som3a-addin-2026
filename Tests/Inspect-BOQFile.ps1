param(
    [string]$FilePath = "$PSScriptRoot\Book2.xlsx"
)

try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false

    $wb = $excel.Workbooks.Open($FilePath)
    
    Write-Host "Workbook: $($wb.Name)"
    Write-Host "Sheets: $($wb.Worksheets.Count)"
    
    for ($i = 1; $i -le $wb.Worksheets.Count; $i++) {
        $ws = $wb.Worksheets($i)
        $usedRows = $ws.UsedRange.Rows.Count
        $usedCols = $ws.UsedRange.Columns.Count
        
        Write-Host ""
        Write-Host ("Sheet " + $i + ": " + $ws.Name + " (" + $usedRows + " rows x " + $usedCols + " cols)")
        
        # Print header row and first 3 data rows
        for ($r = 1; $r -le [Math]::Min(4, $usedRows); $r++) {
            $rowText = ""
            for ($c = 1; $c -le [Math]::Min(8, $usedCols); $c++) {
                $val = $ws.Cells($r, $c).Text
                $rowText = $rowText + " | " + $val
            }
            Write-Host ("  Row " + $r + ":" + $rowText)
        }
    }
    
    $wb.Close($false)
    Write-Host ""
    Write-Host "=== File inspection complete ==="
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
finally {
    if ($null -ne $excel) {
        try { $excel.Quit() } catch { Write-Warning "Excel.Quit failed: $_" }
        try { [Runtime.InteropServices.Marshal]::ReleaseComObject($excel) } catch { Write-Warning "ReleaseComObject failed: $_" }
    }
}
