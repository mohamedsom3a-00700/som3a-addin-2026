param(
    [string]$ExcelPath = "C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE"
)

Write-Host "===== Settings Diagnostic v6: Sidebar Items =====" -ForegroundColor Cyan

$excel = Start-Process -FilePath $ExcelPath -PassThru
Start-Sleep -Seconds 10

$xl = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application")

$addin = $xl.COMAddIns | Where-Object { $_.ProgId -match "Som3a" }
$auto = $addin.Object
$auto.OpenWindow("Home")
Start-Sleep -Seconds 1
$auto.OpenWindow("Add in Setting")
Start-Sleep -Seconds 3

Add-Type -AssemblyName 'UIAutomationClient'
Add-Type -AssemblyName 'UIAutomationTypes'

$uiRoot = [System.Windows.Automation.AutomationElement]::RootElement
$treeScope = [System.Windows.Automation.TreeScope]::Descendants
$treeChildren = [System.Windows.Automation.TreeScope]::Children
$ctrlTypeProp = [System.Windows.Automation.AutomationElement]::ControlTypeProperty

# Find Shell window
$shellWnd = $null
foreach ($w in $uiRoot.FindAll($treeChildren, [System.Windows.Automation.Condition]::TrueCondition)) {
    if ($w.Current.Name -match "Som3a Add-in") { $shellWnd = $w; break }
}

$autoIdProp = [System.Windows.Automation.AutomationElement]::AutomationIdProperty

# ===== Find Settings Sidebar List =====
$cond = New-Object System.Windows.Automation.PropertyCondition($autoIdProp, "SidebarList")
$sidebarList = $shellWnd.FindFirst($treeScope, $cond)

if ($sidebarList) {
    Write-Host "SidebarList found: Bounds=$($sidebarList.Current.BoundingRectangle)" -ForegroundColor Green
    
    # Enumerate ALL children
    $allChildren = $sidebarList.FindAll($treeScope, [System.Windows.Automation.Condition]::TrueCondition)
    Write-Host "Total children (recursive): $($allChildren.Count)" -ForegroundColor Yellow
    
    # Enumerate immediate children
    $immediateChildren = $sidebarList.FindAll($treeChildren, [System.Windows.Automation.Condition]::TrueCondition)
    Write-Host "Immediate children: $($immediateChildren.Count)" -ForegroundColor Yellow
    
    for ($i = 0; $i -lt $immediateChildren.Count; $i++) {
        $child = $immediateChildren[$i]
        $type = $child.Current.ControlType.ProgrammaticName
        $name = $child.Current.Name
        $bounds = $child.Current.BoundingRectangle
        $offscreen = $child.Current.IsOffscreen
        $isEnabled = $child.Current.IsEnabled
        Write-Host "  Child[$i]: Type=$type Name='$name' Bounds=$bounds Offscreen=$offscreen Enabled=$isEnabled" -ForegroundColor Cyan
        
        # If it's a ListItem, check its text children
        if ($type -eq "ControlType.ListItem") {
            $itemTexts = $child.FindAll($treeScope, (New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)))
            Write-Host "    Text children: $($itemTexts.Count)" -ForegroundColor Gray
            foreach ($t in $itemTexts) {
                Write-Host "    '$($t.Current.Name)'" -ForegroundColor White
            }
        }
    }
    
    # Check ListItem count
    $itemCond = New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::ListItem)
    $items = $sidebarList.FindAll($treeScope, $itemCond)
    Write-Host "ListItem elements (recursive): $($items.Count)" -ForegroundColor Green
    for ($i = 0; $i -lt $items.Count; $i++) {
        $item = $items[$i]
        Write-Host "  Item[$i]: Name='$($item.Current.Name)' Bounds=$($item.Current.BoundingRectangle) IsOffscreen=$($item.Current.IsOffscreen)" -ForegroundColor Cyan
        $itemTexts = $item.FindAll($treeScope, (New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)))
        foreach ($t in $itemTexts) {
            Write-Host "    Text: '$($t.Current.Name)'" -ForegroundColor White
        }
    }
} else {
    Write-Host "SidebarList NOT found!" -ForegroundColor Red
}

# ===== Find Shell Sidebar ListBox =====
$shellCond = New-Object System.Windows.Automation.PropertyCondition($autoIdProp, "SidebarListBox")
$shellSidebar = $shellWnd.FindFirst($treeScope, $shellCond)

if ($shellSidebar) {
    Write-Host "`n===== Shell Sidebar ListBox =====" -ForegroundColor Cyan
    Write-Host "SidebarListBox found: Bounds=$($shellSidebar.Current.BoundingRectangle)" -ForegroundColor Green
    
    # Check IMMEDIATE children (these should be GroupItems if grouping works)
    $immediateChildren = $shellSidebar.FindAll($treeChildren, [System.Windows.Automation.Condition]::TrueCondition)
    Write-Host "Immediate children count: $($immediateChildren.Count)" -ForegroundColor Yellow
    for ($i = 0; $i -lt $immediateChildren.Count; $i++) {
        $child = $immediateChildren[$i]
        Write-Host "  Immediate[$i]: Type=$($child.Current.ControlType.ProgrammaticName) Name='$($child.Current.Name)' Bounds=$($child.Current.BoundingRectangle)" -ForegroundColor Cyan
        # Check ALL descendents for text
        $descTexts = $child.FindAll($treeScope, (New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)))
        foreach ($t in $descTexts) { Write-Host "    DescText: '$($t.Current.Name)' Bounds=$($t.Current.BoundingRectangle)" -ForegroundColor White }
    }
    
    # Also search for category header text anywhere in the Shell window
    Write-Host "`nSearching for category header names..." -ForegroundColor Yellow
    $categoryNames = @("Planning", "Navigation", "Home", "Analysis", "Tools", "Management", "Utilities", "Projects", "Settings")
    $allShellTexts = $shellSidebar.FindAll($treeScope, (New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)))
    foreach ($t in $allShellTexts) {
        foreach ($cat in $categoryNames) {
            if ($t.Current.Name -match $cat) {
                Write-Host "  Found category text: '$($t.Current.Name)' Bounds=$($t.Current.BoundingRectangle)" -ForegroundColor Green
            }
        }
    }
} else {
    Write-Host "`nShell SidebarListBox NOT found!" -ForegroundColor Red
}

# ===== Also find DebugCount =====
Write-Host "`n=== DebugCount ===" -ForegroundColor Cyan
$debugCond = New-Object System.Windows.Automation.PropertyCondition($autoIdProp, "DebugCount")
$debugTb = $shellWnd.FindFirst($treeScope, $debugCond)
if ($debugTb) {
    Write-Host "DebugCount: Name='$($debugTb.Current.Name)' Bounds=$($debugTb.Current.BoundingRectangle)" -ForegroundColor Green
} else {
    Write-Host "DebugCount NOT found by AutomationId" -ForegroundColor Yellow
    # Search for "Categories:" in text
    $allText = $shellWnd.FindAll($treeScope, (New-Object System.Windows.Automation.PropertyCondition($ctrlTypeProp, [System.Windows.Automation.ControlType]::Text)))
    foreach ($t in $allText) {
        if ($t.Current.Name -match "Categories|Appearance|Performance|DebugCount") {
            Write-Host "Found: '$($t.Current.Name)' Bounds=$($t.Current.BoundingRectangle)" -ForegroundColor Green
        }
    }
}

if ($auto) { $auto.CloseWindow("Add in Setting") | Out-Null }
Write-Host "`nDone. Press Enter..." -ForegroundColor Yellow
Read-Host
Stop-Process -Id $excel.Id -Force
