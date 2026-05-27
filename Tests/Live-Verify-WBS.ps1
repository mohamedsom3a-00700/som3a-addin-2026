param([string]$ExcelPath)

Add-Type -AssemblyName System.Windows.Forms

function Kill-Excel {
    Get-Process -Name EXCEL -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force }
    Start-Sleep -Seconds 2
}

function Start-Excel {
    if (-not $ExcelPath -or -not (Test-Path $ExcelPath)) {
        $candidates = @(
            "${env:ProgramFiles}\Microsoft Office\root\Office16\EXCEL.EXE",
            "${env:ProgramFiles(x86)}\Microsoft Office\root\Office16\EXCEL.EXE"
        )
        $ExcelPath = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    }
    if (-not $ExcelPath) { throw "Excel not found" }
    $proc = Start-Process -FilePath $ExcelPath -PassThru
    Start-Sleep -Seconds 5
    return $proc
}

function Wait-ForExcelCom {
    for ($i = 0; $i -lt 30; $i++) {
        try { return [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application") }
        catch { }
        Start-Sleep -Seconds 2
    }
    throw "Excel COM not available"
}

Kill-Excel
$excelProc = Start-Excel
$excelApp = Wait-ForExcelCom
$excelApp.Visible = $true
$excelApp.DisplayAlerts = $false
Start-Sleep -Seconds 3

$addin = $null
foreach ($a in $excelApp.COMAddIns) {
    if ($a.Description -like "*Som3a*") { $addin = $a; break }
}
if (-not $addin) { Write-Host "Add-in not loaded!"; exit 1 }
$auto = $addin.Object

Write-Host "=== Setting Alpha mode + Blue Gradient ==="
$auto.SetWBSCodeMode("Alpha") | Out-Null
$auto.SetWBSExportStyle("Blue Gradient") | Out-Null
Write-Host $auto.GetWBSMode()
Write-Host ""

Write-Host "=== Loading Default Construction WBS with Alpha codes ==="
Write-Host ""
$result = $auto.GetTemplatePreviewData("Default Construction WBS")
Write-Host $result
Write-Host ""

Write-Host "Press ENTER to close Excel..."
Read-Host

$excelApp.Quit()
[System.Runtime.InteropServices.Marshal]::ReleaseComObject($excelApp) | Out-Null
Kill-Excel
