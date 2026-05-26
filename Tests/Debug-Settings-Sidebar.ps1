param(
    [string]$ExcelPath = "C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE"
)

Write-Host "===== Settings Sidebar Debug =====" -ForegroundColor Cyan
Write-Host "Launching Excel..." -ForegroundColor Gray

$excel = Start-Process -FilePath $ExcelPath -PassThru
Start-Sleep -Seconds 8

Write-Host "Excel PID: $($excel.Id)" -ForegroundColor Green

Add-Type -AssemblyName 'UIAutomationClient'
Add-Type -AssemblyName 'UIAutomationTypes'

$uiRoot = [System.Windows.Automation.AutomationElement]::RootElement
$nameProp = [System.Windows.Automation.AutomationElement]::NameProperty
$ctrlTypeProp = [System.Windows.Automation.AutomationElement]::ControlTypeProperty
$autoIdProp = [System.Windows.Automation.AutomationElement]::AutomationIdProperty
$treeScope = [System.Windows.Automation.TreeScope]::Descendants

Write-Host "`nChecking for Settings window..." -ForegroundColor Yellow

$cond = New-Object System.Windows.Automation.PropertyCondition($nameProp, "Settings")
$settingsWnd = $uiRoot.FindFirst($treeScope, $cond)

if (-not $settingsWnd) {
    Write-Host "Settings window not found yet. Will try opening via automation..." -ForegroundColor Yellow
    Write-Host "Available top-level windows:" -ForegroundColor Gray
    $topWnd = $uiRoot.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($w in $topWnd) {
        Write-Host "  '$($w.Current.Name)'" -ForegroundColor DarkGray
    }
    
    # Try to find the add-in shell window
    $shellCond = New-Object System.Windows.Automation.PropertyCondition($nameProp, "Som3a Add-in (Shell)")
    $shellWnd = $uiRoot.FindFirst($treeScope, $shellCond)
    if ($shellWnd) {
        Write-Host "Found Shell window, trying to find navigation..." -ForegroundColor Green
    }
    
    # Try clicking the ribbon to open settings
    Write-Host "`nCannot open settings via COM automation in this script." -ForegroundColor Red
    Write-Host "Please manually open Settings page in Excel, then press Enter..." -ForegroundColor Yellow
    Read-Host
}

# Retry finding settings
$settingsWnd = $uiRoot.FindFirst($treeScope, $cond)
if (-not $settingsWnd) {
    Write-Host "Settings window still not found. Exiting." -ForegroundColor Red
    Stop-Process -Id $excel.Id -Force
    exit 1
}

Write-Host "`nSettings window found!" -ForegroundColor Green
Write-Host "Bounds: $($settingsWnd.Current.BoundingRectangle)" -ForegroundColor Gray

# List all text elements
Write-Host "`n--- All Text Elements in Settings Window ---" -ForegroundColor Cyan
$textCond = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)
$texts = $settingsWnd.FindAll($treeScope, $textCond)
Write-Host "Found $($texts.Count) text elements" -ForegroundColor Yellow
foreach ($t in $texts) {
    $txt = $t.Current.Name
    if ($txt -match '\S') {
        $bounds = $t.Current.BoundingRectangle
        Write-Host "  Text: '$txt' Bounds: $bounds" -ForegroundColor White
    }
}

# Find ListBox by AutomationId
Write-Host "`n--- Sidebar ListBox Check ---" -ForegroundColor Cyan
$listCond = New-Object System.Windows.Automation.PropertyCondition($autoIdProp, "SidebarList")
$listBox = $settingsWnd.FindFirst($treeScope, $listCond)

if ($listBox) {
    Write-Host "Sidebar ListBox found!" -ForegroundColor Green
    Write-Host "Bounds: $($listBox.Current.BoundingRectangle)" -ForegroundColor Gray
    Write-Host "IsEnabled: $($listBox.Current.IsEnabled)" -ForegroundColor Gray
    Write-Host "IsOffscreen: $($listBox.Current.IsOffscreen)" -ForegroundColor Gray
    
    $itemCond = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::ListItem)
    $items = $listBox.FindAll([System.Windows.Automation.TreeScope]::Children, $itemCond)
    Write-Host "Items count: $($items.Count)" -ForegroundColor Green
    for ($i = 0; $i -lt $items.Count; $i++) {
        $item = $items[$i]
        Write-Host "  Item[$i]:"
        Write-Host "    Name: '$($item.Current.Name)'"
        Write-Host "    Bounds: $($item.Current.BoundingRectangle)"
        Write-Host "    IsOffscreen: $($item.Current.IsOffscreen)"
    }
} else {
    Write-Host "Sidebar ListBox NOT found by AutomationId" -ForegroundColor Red
    
    # Find any list controls
    $listCond2 = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::List)
    $allLists = $settingsWnd.FindAll($treeScope, $listCond2)
    Write-Host "Found $($allLists.Count) list controls in settings window" -ForegroundColor Yellow
    for ($i = 0; $i -lt $allLists.Count; $i++) {
        $l = $allLists[$i]
        Write-Host "  List[$i]: AutoId='$($l.Current.AutomationId)' Name='$($l.Current.Name)' Bounds=$($l.Current.BoundingRectangle)" -ForegroundColor Gray
    }
}

# Cleanup
Write-Host "`nDone. Press Enter to close Excel..." -ForegroundColor Yellow
Read-Host
Stop-Process -Id $excel.Id -Force
Write-Host "Excel closed." -ForegroundColor Gray
