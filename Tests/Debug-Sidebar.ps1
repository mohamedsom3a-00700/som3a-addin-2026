Add-Type -AssemblyName 'UIAutomationClient'
Add-Type -AssemblyName 'UIAutomationTypes'

$proc = Get-Process -Name EXCEL -ErrorAction SilentlyContinue
if (-not $proc) {
    Write-Host "No Excel process found" -ForegroundColor Red
    exit 1
}
Write-Host "Excel PID: $($proc.Id)" -ForegroundColor Green

$uiRoot = [System.Windows.Automation.AutomationElement]::RootElement
$nameProp = [System.Windows.Automation.AutomationElement]::NameProperty
$ctrlTypeProp = [System.Windows.Automation.AutomationElement]::ControlTypeProperty
$autoIdProp = [System.Windows.Automation.AutomationElement]::AutomationIdProperty
$treeScope = [System.Windows.Automation.TreeScope]::Descendants

$cond = New-Object System.Windows.Automation.PropertyCondition($nameProp, "Settings")
$settingsWnd = $uiRoot.FindFirst($treeScope, $cond)

if (-not $settingsWnd) {
    Write-Host "Settings window NOT found" -ForegroundColor Red
    $allWnd = $uiRoot.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($w in $allWnd) {
        Write-Host "  Window: '$($w.Current.Name)'" -ForegroundColor Gray
    }
    exit 1
}

Write-Host "Settings window found: bounds=$($settingsWnd.Current.BoundingRectangle)" -ForegroundColor Green

# Find all text elements in the window
$textCond = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)
$texts = $settingsWnd.FindAll($treeScope, $textCond)
Write-Host "Text elements found: $($texts.Count)" -ForegroundColor Yellow
foreach ($t in $texts) {
    $txt = $t.Current.Name
    if ($txt -match '\S') {
        Write-Host "  Text: '$txt'" -ForegroundColor Gray
    }
}

# Find ListBox by AutomationId
$listCond = New-Object System.Windows.Automation.PropertyCondition($autoIdProp, "SidebarList")
$listBox = $settingsWnd.FindFirst($treeScope, $listCond)

if ($listBox) {
    Write-Host "Sidebar ListBox found: bounds=$($listBox.Current.BoundingRectangle)" -ForegroundColor Green
    $itemCond = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::ListItem)
    $items = $listBox.FindAll([System.Windows.Automation.TreeScope]::Children, $itemCond)
    Write-Host "ListBox items: $($items.Count)" -ForegroundColor Green
    for ($i = 0; $i -lt $items.Count; $i++) {
        $item = $items[$i]
        Write-Host "  Item[$i]: Name='$($item.Current.Name)' IsEnabled=$($item.Current.IsEnabled) Bounds=$($item.Current.BoundingRectangle)" -ForegroundColor Cyan
    }
} else {
    Write-Host "Sidebar ListBox NOT found by AutomationId" -ForegroundColor Red
}
