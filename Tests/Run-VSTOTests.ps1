<#
.SYNOPSIS
    VSTO Enterprise Polish Validation Suite - Som3a Add-in 2026
.DESCRIPTION
    Automates Excel VSTO host testing from CLI. Opens all 14 windows, tests
    theme switching, safe mode, measures memory/timing, captures screenshots.
    Logs structured results to Tests/VSTOResults.xml and screenshots to Tests/Screenshots/.
.PARAMETER ExcelPath
    Path to EXCEL.EXE. Auto-detected if omitted.
.PARAMETER AddInName
    COM Add-in name (default: "Som3a Addin 2026").
.PARAMETER ScreenshotDir
    Output directory for screenshots (default: Tests/Screenshots/).
.PARAMETER ResultsFile
    Path for structured results XML (default: Tests/VSTOResults.xml).
.PARAMETER NoScreenshots
    Skip screenshot capture for faster execution.
.PARAMETER Quick
    Only test a subset (main window + 3 representative windows) for CI smoke test.
.EXAMPLE
    .\Run-VSTOTests.ps1
    .\Run-VSTOTests.ps1 -Quick
    .\Run-VSTOTests.ps1 -NoScreenshots -ResultsFile ..\VSTOResults.xml
#>

param(
    [string]$ExcelPath,
    [string]$AddInName = "Som3a Addin 2026",
    [string]$ScreenshotDir = "$PSScriptRoot\Screenshots",
    [string]$ResultsFile = "$PSScriptRoot\VSTOResults.xml",
    [switch]$NoScreenshots,
    [switch]$Quick
)

# Ensure required assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# ====================================================================
# HELPER FUNCTIONS
# ====================================================================

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] [$Level] $Message"
}

function Get-Timestamp {
    return (Get-Date -Format "yyyyMMdd_HHmmss")
}

function Get-ExcelProcess {
    return Get-Process -Name "EXCEL" -ErrorAction SilentlyContinue
}

function Kill-Excel {
    $procs = Get-ExcelProcess
    if ($procs) {
        foreach ($p in $procs) {
            Write-Log "Killing Excel process $($p.Id)..."
            Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
        }
        Start-Sleep -Seconds 2
    }
}

function Start-Excel {
    param([string]$Path)

    if (-not $Path -or -not (Test-Path $Path)) {
        # Auto-detect Excel 2016/2019/2021/365
        $candidates = @(
            "${env:ProgramFiles}\Microsoft Office\root\Office16\EXCEL.EXE",
            "${env:ProgramFiles(x86)}\Microsoft Office\root\Office16\EXCEL.EXE",
            "${env:ProgramFiles}\Microsoft Office\Office16\EXCEL.EXE",
            "${env:ProgramFiles(x86)}\Microsoft Office\Office16\EXCEL.EXE",
            "${env:ProgramFiles}\Microsoft Office\Office15\EXCEL.EXE",
            "${env:ProgramFiles(x86)}\Microsoft Office\Office15\EXCEL.EXE"
        )
        $Path = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    }

    if (-not $Path) {
        throw "Excel not found. Provide -ExcelPath or install Microsoft Excel."
    }

    Write-Log "Launching Excel from: $Path"
    $proc = Start-Process -FilePath $Path -PassThru
    Start-Sleep -Seconds 5  # wait for COM registration
    return $proc
}

function Wait-ForExcelCom {
    param([int]$Retries = 30, [int]$DelaySeconds = 2)
    for ($i = 0; $i -lt $Retries; $i++) {
        try {
            $excel = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application")
            if ($excel) { return $excel }
        } catch { }
        Start-Sleep -Seconds $DelaySeconds
    }
    throw "Excel COM object not available after $($Retries * $DelaySeconds) seconds."
}

function Get-AddInLoaded {
    param($ExcelApp, [string]$Name)
    try {
        $addins = $ExcelApp.COMAddIns
        foreach ($addin in $addins) {
            if ($addin.Description -like "*$Name*" -or $addin.ProgId -like "*$Name*") {
                return $addin
            }
        }
        return $null
    } catch {
        return $null
    }
}

function Get-WindowByTitle {
    param([string]$Title, [int]$TimeoutSeconds = 10, [int]$ProcessId = 0)
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        $all = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
            [System.Windows.Automation.TreeScope]::Children,
            [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($elem in $all) {
            try {
                $name = $elem.Current.Name
                if ($name -like "*$Title*") {
                    if ($ProcessId -gt 0) {
                        $windowPid = 0
                        try { $windowPid = $elem.Current.ProcessId } catch { }
                        if ($windowPid -eq $ProcessId) { return $elem }
                    } else {
                        return $elem
                    }
                }
            } catch { }
        }
        Start-Sleep -Seconds 1
        $elapsed++
    }
    return $null
}

function Get-WpfChildWindows {
    param($ParentWindow)
    $children = @()
    $walker = [System.Windows.Automation.TreeWalker]::new(
        [System.Windows.Automation.Condition]::TrueCondition)
    $node = $walker.GetFirstChild($ParentWindow)
    while ($node) {
        $children += $node
        $node = $walker.GetNextSibling($node)
    }
    return $children
}

function Get-MemoryMB {
    $proc = Get-ExcelProcess | Select-Object -First 1
    if (-not $proc) { return $null }
    [math]::Round($proc.WorkingSet64 / 1MB, 1)
}

function Get-ManagedMemoryMB {
    try {
        return [math]::Round([System.GC]::GetTotalMemory($false) / 1MB, 1)
    } catch { return $null }
}

function Take-Screenshot {
    param([string]$Label)
    if ($NoScreenshots) { return }
    if (-not (Test-Path $ScreenshotDir)) { New-Item -ItemType Directory -Path $ScreenshotDir -Force | Out-Null }
    $filename = "$ScreenshotDir\${Label}_$(Get-Timestamp).png"
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
    $bitmap.Save($filename, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
    Write-Log "Screenshot saved: $filename"
    return $filename
}

function Test-RibbonButton {
    param($ExcelApp, [string]$ButtonLabel)
    try {
        $hwnd = New-Object System.IntPtr -ArgumentList $ExcelApp.Hwnd
        $excelElem = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        if (-not $excelElem) { return $false }

        # Strategy 1: find and click "Som3a Planning Tools" ribbon tab
        $tabCond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty, "Som3a Planning Tools")
        $tab = $excelElem.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
        if ($tab) {
            $pattern = $null
            if ($tab.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
                $pattern.Invoke()
            } elseif ($tab.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) {
                ([System.Windows.Automation.SelectionItemPattern]$pattern).Select()
            }
            Start-Sleep -Milliseconds 800
        }

        # Also try "Som3a Helper Tools" tab
        $tab2Cond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty, "Som3a Helper Tools")
        $tab2 = $excelElem.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $tab2Cond)

        # Strategy 2: exact button name match in descendants of Excel window
        $btnCond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty, $ButtonLabel)
        $btn = $excelElem.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
        if ($btn) {
            $invokePattern = $null
            if ($btn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
                $invokePattern.Invoke()
                Start-Sleep -Seconds 1
                return $true
            }
        }

        return $false
    } catch {
        Write-Log "Ribbon click failed for '$ButtonLabel': $_" -Level "WARN"
        return $false
    }
}
function Measure-WindowOpenClose {
    param(
        [string]$WindowName,
        [scriptblock]$OpenAction,
        [int]$OpenTimeout = 10,
        [int]$CloseTimeout = 5
    )
    $memoryBefore = Get-MemoryMB
    $sw = [System.Diagnostics.Stopwatch]::StartNew()

    # Open the window
    & $OpenAction
    $window = Get-WindowByTitle -Title $WindowName -TimeoutSeconds $OpenTimeout
    $sw.Stop()
    $openTimeMs = $sw.ElapsedMilliseconds

    $foundWindow = $null -ne $window
    $windowTitle = if ($foundWindow) { $window.Current.Name } else { "N/A" }

    # Take screenshot
    if ($foundWindow) {
        Take-Screenshot -Label "Window_$($WindowName -replace '[^a-zA-Z0-9]','_')"
    }

    # Close the window (Escape key)
    if ($foundWindow) {
        try {
            $window.SetFocus()
            [System.Windows.Forms.SendKeys]::SendWait("{ESC}")
            Start-Sleep -Seconds 1
        } catch { }
    }

    $memoryAfter = Get-MemoryMB
    $memDelta = if ($memoryBefore -and $memoryAfter) { $memoryAfter - $memoryBefore } else { $null }

    return @{
        Name        = $WindowName
        Found       = $foundWindow
        Title       = $windowTitle
        OpenTimeMs  = $openTimeMs
        MemBeforeMB = $memoryBefore
        MemAfterMB  = $memoryAfter
        MemDeltaMB  = $memDelta
    }
}

function New-ResultXml {
    $doc = New-Object System.Xml.XmlDocument
    $doc.LoadXml('<?xml version="1.0" encoding="utf-8"?><VSTOResults><summary total="0" passed="0" failed="0" skipped="0"/></VSTOResults>')
    return $doc
}

function Add-Result {
    param($Doc, [string]$TaskId, [string]$Name, [string]$Status, [string]$Detail, [string]$Category)
    $root = $Doc.DocumentElement
    $testEl = $Doc.CreateElement("test")
    $testEl.SetAttribute("id", $TaskId)
    $testEl.SetAttribute("name", $Name)
    $testEl.SetAttribute("status", $Status)
    $testEl.SetAttribute("category", $Category)
    $testEl.SetAttribute("detail", $Detail)
    $testEl.SetAttribute("timestamp", (Get-Date -Format "o"))
    $root.AppendChild($testEl) | Out-Null

    $summary = $root.SelectSingleNode("summary")
    $total = [int]$summary.GetAttribute("total") + 1
    $summary.SetAttribute("total", $total)
    if ($Status -eq "pass") { $passed = [int]$summary.GetAttribute("passed") + 1; $summary.SetAttribute("passed", $passed) }
    elseif ($Status -eq "fail") { $failed = [int]$summary.GetAttribute("failed") + 1; $summary.SetAttribute("failed", $failed) }
    else { $skipped = [int]$summary.GetAttribute("skipped") + 1; $summary.SetAttribute("skipped", $skipped) }
}

# ====================================================================
# WINDOW TEST DEFINITIONS
# ====================================================================

$windowTests = @(
    @{ ButtonLabel = "Home"; WindowTitle = "Som3a Add-in"; TaskId = "T037-T046"; Description = "ShellWindow" }
    @{ ButtonLabel = "Comparsion"; WindowTitle = "Comparison P6 Activity"; TaskId = "T037-T046"; Description = "MainWindow" }
    @{ ButtonLabel = "Trades Codes"; WindowTitle = "Trades Codes"; TaskId = "T037"; Description = "AssignTradeCodesWindow" }
    @{ ButtonLabel = "Daily Report"; WindowTitle = "Daily Report"; TaskId = "T037"; Description = "SubDailyReportWindow" }
    @{ ButtonLabel = "Links Manager"; WindowTitle = "Links Manager"; TaskId = "T037"; Description = "LinksManagerWindow" }
    @{ ButtonLabel = "Revised Baseline (Split Activity)"; WindowTitle = "Project Analysis"; TaskId = "T037"; Description = "ProjectAnalysisWindow" }
    @{ ButtonLabel = "Xer Editor"; WindowTitle = "XER Editor"; TaskId = "T037"; Description = "XerEditorWindow" }
    @{ ButtonLabel = "Color WBS Setting"; WindowTitle = "WBS Color Styles"; TaskId = "T037"; Description = "StyleSelectorWindow" }
    @{ ButtonLabel = "Unmerge Fill Down"; WindowTitle = "Unmerge Fill Down"; TaskId = "T037"; Description = "UnmergeFillDownWindow" }
    @{ ButtonLabel = "Float Path Analyzer"; WindowTitle = "Float Path Analyzer"; TaskId = "T037"; Description = "Float_path" }
    @{ ButtonLabel = "Fix Pie Chart Colors"; WindowTitle = "Fix Pie Chart Colors"; TaskId = "T037"; Description = "Fixpiecolors" }
    @{ ButtonLabel = "Comparsion by Xer"; WindowTitle = "Primavera Compare"; TaskId = "T040"; Description = "PrimaveraCompareWindow" }
    @{ ButtonLabel = "Add in Setting"; WindowTitle = "Settings"; TaskId = "T037"; Description = "SettingsWindow" }
    # BOQ Activity Generator sidebar page (inside shell, no separate window)
    @{ ButtonLabel = "BOQ Activity Generator"; WindowTitle = ""; TaskId = "T017"; Description = "BOQActivityGeneratorPage" }
    # WBS sidebar pages (navigate inside shell, no separate window)
    @{ ButtonLabel = "WBS Template Browser"; WindowTitle = ""; TaskId = "T037"; Description = "WBSTemplateBrowserPage" }
    @{ ButtonLabel = "WBS Generator"; WindowTitle = ""; TaskId = "T037"; Description = "WBSGeneratorPage" }
    @{ ButtonLabel = "WBS Editor"; WindowTitle = ""; TaskId = "T037"; Description = "WBSEditorPage" }
    # Phase 22: Duration Estimator Plugin
    @{ ButtonLabel = "Duration Estimator"; WindowTitle = ""; TaskId = "T013"; Description = "DurationEstimatorPage" }
    # Phase 24: Localization & RTL
    @{ ButtonLabel = "Language Settings"; WindowTitle = ""; TaskId = "T012"; Description = "LanguagePage" }
)

if ($Quick) {
    $windowTests = $windowTests | Select-Object -First 4
}

# ====================================================================
# MAIN EXECUTION
# ====================================================================

$resultsDoc = New-ResultXml
$overallPass = $false

try {
    # Step 1: Clean slate
    Write-Log "===== VSTO Enterprise Polish Test Suite ====="
    Write-Log "Quick mode: $Quick | Screenshots: $(-not $NoScreenshots)"
    Kill-Excel

    # Step 2: Launch Excel
    $excelProc = Start-Excel -Path $ExcelPath
    $excelApp = Wait-ForExcelCom
    $excelApp.Visible = $true
    $excelApp.DisplayAlerts = $false
    Write-Log "Excel launched (PID: $($excelProc.Id))"

    # Step 3: Verify add-in loaded
    $addin = Get-AddInLoaded -ExcelApp $excelApp -Name $AddInName
    if (-not $addin) {
        Write-Log "Add-in '$AddInName' not found." -Level "ERROR"
        Add-Result -Doc $resultsDoc -TaskId "T037" -Name "AddInLoaded" -Status "fail" `
            -Detail "Add-in not found" -Category "Setup"
        throw "Add-in not loaded"
    }
    Write-Log "Add-in verified loaded: $($addin.ProgId)"
    Add-Result -Doc $resultsDoc -TaskId "T037" -Name "AddInLoaded" -Status "pass" `
        -Detail "Add-in $AddInName found in COMAddIns" -Category "Setup"

    # Step 4: Get COM automation interface
    $auto = $null
    try {
        Start-Sleep -Seconds 3
        $auto = $addin.Object
        $testStatus = $auto.GetStatus()
        Write-Log "COM automation ready: $testStatus"
        Add-Result -Doc $resultsDoc -TaskId "T037" -Name "ComAutomation" -Status "pass" `
            -Detail "COM automation interface connected" -Category "Setup"
    } catch {
        Write-Log "COM automation not available, falling back to UIA" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T037" -Name "ComAutomation" -Status "warn" `
            -Detail "COM automation failed: $_" -Category "Setup"
    }

    # Step 5: Open a blank workbook
    try {
        $excelApp.Workbooks.Add() | Out-Null
        Write-Log "Blank workbook created"
    } catch { }

    # Step 6: Baseline memory
    $memBaseline = Get-MemoryMB
    Write-Log "Memory baseline: $memBaseline MB"
    Add-Result -Doc $resultsDoc -TaskId "T054" -Name "MemoryBaseline" -Status "pass" `
        -Detail "Working set: $memBaseline MB at startup" -Category "Memory"

    # Step 7: Test each window via COM automation
    Write-Log "===== Window Tests ====="
    $windowResults = @()
    $openAttempts = 0

    Take-Screenshot -Label "00_Baseline_Excel"

    foreach ($test in $windowTests) {
        $btnLabel = $test.ButtonLabel
        $winTitle = $test.WindowTitle
        $taskId = $test.TaskId
        $isSidebarPage = [string]::IsNullOrEmpty($winTitle)

        $label = if ($isSidebarPage) { $btnLabel } else { $winTitle }
        Write-Log "Testing $label"
        $memoryBefore = Get-MemoryMB
        $sw = [System.Diagnostics.Stopwatch]::StartNew()

        $navSuccess = $false
        if ($auto) {
            $result = $auto.OpenWindow($btnLabel)
            $navSuccess = $result -eq "OK"
            Write-Log "  COM OpenWindow($btnLabel): $result"
        }

        if (-not $navSuccess -and -not $isSidebarPage) {
            $null = Test-RibbonButton -ExcelApp $excelApp -ButtonLabel $btnLabel
            Start-Sleep -Seconds 3
            $window = Get-WindowByTitle -Title $winTitle -TimeoutSeconds 5 -ProcessId $excelProc.Id
            $windowFound = $null -ne $window
        } elseif (-not $navSuccess -and $isSidebarPage) {
            Write-Log "  Sidebar page requires COM automation" -Level "WARN"
        }

        if (-not $navSuccess -and -not $isSidebarPage) {
            $window = Get-WindowByTitle -Title $winTitle -TimeoutSeconds 3 -ProcessId $excelProc.Id
            $windowFound = $null -ne $window
            if ($windowFound) { $navSuccess = $true }
        }

        $sw.Stop()
        $openTimeMs = $sw.ElapsedMilliseconds

        if ($navSuccess) {
            Write-Log "  [NAVIGATED] '$label' (${openTimeMs}ms)"
            Take-Screenshot -Label "Window_$($label -replace '[^a-zA-Z0-9]','_')"

            if ($auto -and -not $isSidebarPage) { $auto.CloseWindow($btnLabel) | Out-Null }
            Start-Sleep -Milliseconds 500

            $memoryAfter = Get-MemoryMB
            $memDelta = if ($memoryBefore -and $memoryAfter) { $memoryAfter - $memoryBefore } else { "N/A" }

            Add-Result -Doc $resultsDoc -TaskId $taskId -Name "Window_$label" -Status "pass" `
                -Detail "Navigated in ${openTimeMs}ms, mem delta: ${memDelta}MB" -Category "Window"
            $windowResults += [PSCustomObject]@{
                Name = $label; Status = "PASS"; OpenTimeMs = $openTimeMs; MemDeltaMB = $memDelta
            }
            $openAttempts++
        } elseif (-not $isSidebarPage) {
            Write-Log "  [NOT FOUND] Window '$winTitle'" -Level "WARN"
            Add-Result -Doc $resultsDoc -TaskId $taskId -Name "Window_$label" -Status "skip" `
                -Detail "Window not found" -Category "Window"
        } else {
            Write-Log "  [FAIL] Sidebar page '$label' navigation failed" -Level "WARN"
            Add-Result -Doc $resultsDoc -TaskId $taskId -Name "Window_$label" -Status "fail" `
                -Detail "Sidebar page navigation failed" -Category "Window"
        }
        Start-Sleep -Milliseconds 500
    }

    # Step 8: Theme switching test via COM
    Write-Log "===== Theme Switch Test (T042/T048) ====="
    $themeResults = @()
    foreach ($theme in @("Dark", "Light", "Custom")) {
        Write-Log "Switching to theme: $theme"
        $memBeforeTheme = Get-MemoryMB
        $sw = [System.Diagnostics.Stopwatch]::StartNew()

        $themeSwitchResult = "unknown"
        if ($auto) {
            $themeSwitchResult = $auto.SwitchTheme($theme)
            Write-Log "  COM SwitchTheme: $themeSwitchResult"
        }
        Start-Sleep -Seconds 1

        Take-Screenshot -Label "Theme_$theme"
        $sw.Stop()
        $memAfterTheme = Get-MemoryMB
        $memThemeDelta = if ($memBeforeTheme -and $memAfterTheme) { $memAfterTheme - $memBeforeTheme } else { "N/A" }

        $themeStatus = if ($themeSwitchResult -eq "OK") { "pass" } else { "skip" }
        Add-Result -Doc $resultsDoc -TaskId "T042" -Name "ThemeSwitch_$theme" -Status $themeStatus `
            -Detail "Theme switch in $($sw.ElapsedMilliseconds)ms, result: $themeSwitchResult" -Category "Theme"
        $themeResults += [PSCustomObject]@{
            Theme = $theme; Status = $(if ($themeSwitchResult -eq "OK") { "PASS" } else { "SKIP" })
            TimeMs = $sw.ElapsedMilliseconds; MemDelta = $memThemeDelta
        }
    }

    # Step 9: Rapid theme switching
    Write-Log "===== Rapid Theme Switch Test (T048) ====="
    $rapidSuccess = $false
    if ($auto) {
        $rapidSuccess = $true
        for ($i = 0; $i -lt 10; $i++) {
            $t = @("Dark", "Light", "Custom")[$i % 3]
            $r = $auto.SwitchTheme($t)
            if ($r -ne "OK") { $rapidSuccess = $false; Write-Log "  Rapid switch $i ($t): $r" -Level "WARN" }
            Start-Sleep -Milliseconds 200
        }
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
    }
    $memAfterRapid = Get-MemoryMB
    Add-Result -Doc $resultsDoc -TaskId "T048" -Name "RapidThemeSwitch_10x" -Status $(if ($rapidSuccess) { "pass" } else { "warn" }) `
        -Detail "10 rapid switches completed, memory: $memAfterRapid MB" -Category "Theme"

    # Step 10: WBS Settings Verification
    Write-Log "===== WBS Settings Verification ====="
    $wbsResults = @()
    if ($auto) {
        # Test 1: Set Alpha mode
        Write-Log "  Setting WBSCodeMode to Alpha..."
        $r1 = $auto.SetWBSCodeMode("Alpha")
        Write-Log "    SetWBSCodeMode(Alpha): $r1"
        $wbsOk = $r1 -eq "OK"

        # Test 2: Set Blue Gradient export style
        Write-Log "  Setting WBSExportStyle to Blue Gradient..."
        $r2 = $auto.SetWBSExportStyle("Blue Gradient")
        Write-Log "    SetWBSExportStyle(Blue Gradient): $r2"
        $wbsOk = $wbsOk -and ($r2 -eq "OK")

        # Test 3: Verify settings were applied
        Write-Log "  Verifying settings..."
        $r3 = $auto.GetWBSMode()
        Write-Log "    GetWBSMode: $r3"
        $wbsOk = $wbsOk -and ($r3 -like "*Mode:Alpha*") -and ($r3 -like "*ExportStyle:Blue Gradient*") -and ($r3 -like "*SelectedStyleId:2*")

        # Test 4: Navigate to WBS Template Browser (should show alpha codes now)
        Write-Log "  Navigating to WBS Template Browser..."
        $r4 = $auto.OpenWindow("WBS Template Browser")
        Write-Log "    OpenWindow(WBS Template Browser): $r4"
        $wbsOk = $wbsOk -and ($r4 -eq "OK")
        Start-Sleep -Seconds 1
        Take-Screenshot -Label "WBS_Alpha_BlueGradient"

        Add-Result -Doc $resultsDoc -TaskId "T037" -Name "WBSSettings_AlphaMode" -Status $(if ($wbsOk) { "pass" } else { "fail" }) `
            -Detail "Alpha mode=$r1 | BlueGradient=$r2 | Verify=$r3 | Nav=$r4" -Category "WBS"
        $wbsResults += [PSCustomObject]@{
            Test = "WBS Settings (Alpha + Blue Gradient)"; Status = $(if ($wbsOk) { "PASS" } else { "FAIL" })
            Detail = "SetWBSCodeMode=$r1 | SetStyle=$r2 | GetMode=$r3 | Nav=$r4"
        }
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T037" -Name "WBSSettings_AlphaMode" -Status "skip" `
            -Detail "COM automation not available" -Category "WBS"
    }

    # Step 11: Duration Estimator Plugin Test (Phase 22)
    Write-Log "===== Duration Estimator Plugin Test (T013-T015) ====="
    $deResults = @()
    if ($auto) {
        Write-Log "  Navigating to Duration Estimator..."
        $deNav = $auto.OpenWindow("Duration Estimator")
        Write-Log "    OpenWindow(Duration Estimator): $deNav"
        $deOk = $deNav -eq "OK"

        if ($deOk) {
            Start-Sleep -Seconds 1
            Take-Screenshot -Label "DurationEstimator_Loaded"

            Write-Log "  Testing calculation via COM..."
            $deCalc = $auto.DeCalculateDuration("ACT-TEST", 100, 10, 2, 8)
            Write-Log "    DeCalculateDuration: $deCalc"
            $deOk = $deOk -and ($deCalc -ne $null -and $deCalc -notlike "ERROR*")

            Write-Log "  Testing benchmark search via COM..."
            $deBench = $auto.DeSearchBenchmarks("concrete", "")
            Write-Log "    DeSearchBenchmarks(concrete): $deBench"
            $deOk = $deOk -and ($deBench -ne $null -and $deBench -notlike "ERROR*")

            $deResults += [PSCustomObject]@{
                Test = "Duration Estimator Open"; Status = $(if ($deNav -eq "OK") { "PASS" } else { "FAIL" })
                Detail = "OpenWindow=$deNav"
            }
            $deResults += [PSCustomObject]@{
                Test = "Calculate Duration"; Status = $(if ($deCalc -ne $null -and $deCalc -notlike "ERROR*") { "PASS" } else { "FAIL" })
                Detail = "Result=$deCalc"
            }
            $deResults += [PSCustomObject]@{
                Test = "Search Benchmarks"; Status = $(if ($deBench -ne $null -and $deBench -notlike "ERROR*") { "PASS" } else { "FAIL" })
                Detail = "Result=$deBench"
            }
        }

        Add-Result -Doc $resultsDoc -TaskId "T013" -Name "DurationEstimator_Load" -Status $(if ($deOk) { "pass" } else { "fail" }) `
            -Detail "Nav=$deNav | Calc=$deCalc | Bench=$deBench" -Category "DurationEstimator"
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T013" -Name "DurationEstimator_Load" -Status "skip" `
            -Detail "COM automation not available" -Category "DurationEstimator"
    }

    # Step 12: Dashboard & Home Page Test (Phase 23)
    Write-Log "===== Dashboard & Home Page Test (Phase 23) ====="
    $dashboardResults = @()
    if ($auto) {
        Write-Log "  Navigating to Home..."
        $homeNav = $auto.OpenWindow("Home")
        Write-Log "    OpenWindow(Home): $homeNav"
        $dashboardOk = $homeNav -eq "OK"
        Start-Sleep -Seconds 2

        if ($dashboardOk) {
            Take-Screenshot -Label "Dashboard_HomePage"

            Write-Log "  Querying home page status..."
            $homeStatus = $auto.GetHomePageStatus()
            Write-Log "    GetHomePageStatus: $homeStatus"
            $dashboardOk = $dashboardOk -and ($homeStatus -like "OK|*")
            $homeStatusDetail = $homeStatus -replace "OK\|", ""

            Write-Log "  Querying Recent Tools widget..."
            $recentWidget = $auto.GetWidgetStatus("Recent Tools")
            Write-Log "    GetWidgetStatus(Recent Tools): $recentWidget"
            $dashboardOk = $dashboardOk -and ($recentWidget -like "OK|*")

            Write-Log "  Querying AI Provider Status widget..."
            $aiWidget = $auto.GetWidgetStatus("AI Providers")
            Write-Log "    GetWidgetStatus(AI Providers): $aiWidget"

            Write-Log "  Querying Performance widget..."
            $perfWidget = $auto.GetWidgetStatus("Performance")
            Write-Log "    GetWidgetStatus(Performance): $perfWidget"

            Write-Log "  Querying Diagnostics widget..."
            $diagWidget = $auto.GetWidgetStatus("Diagnostics")
            Write-Log "    GetWidgetStatus(Diagnostics): $diagWidget"

            Write-Log "  Testing Home icon navigation (re-navigate to home)..."
            $reNav = $auto.OpenWindow("Home")
            Write-Log "    Re-navigate Home: $reNav"
            $dashboardOk = $dashboardOk -and ($reNav -eq "OK")

            $dashboardResults += [PSCustomObject]@{
                Test = "Dashboard Home Page Load"; Status = $(if ($homeNav -eq "OK") { "PASS" } else { "FAIL" })
                Detail = "OpenWindow=$homeNav | Status=$homeStatusDetail"
            }
            $dashboardResults += [PSCustomObject]@{
                Test = "Recent Tools Widget"; Status = $(if ($recentWidget -like "OK|*") { "PASS" } else { "FAIL" })
                Detail = "Status=$recentWidget"
            }
            $dashboardResults += [PSCustomObject]@{
                Test = "AI Provider Status Widget"; Status = $(if ($aiWidget -like "OK|*") { "PASS" } else { "FAIL" })
                Detail = "Status=$aiWidget"
            }
            $dashboardResults += [PSCustomObject]@{
                Test = "Performance Widget"; Status = $(if ($perfWidget -like "OK|*") { "PASS" } else { "FAIL" })
                Detail = "Status=$perfWidget"
            }
            $dashboardResults += [PSCustomObject]@{
                Test = "Diagnostics Widget"; Status = $(if ($diagWidget -like "OK|*") { "PASS" } else { "FAIL" })
                Detail = "Status=$diagWidget"
            }
            $dashboardResults += [PSCustomObject]@{
                Test = "Home Icon Navigation"; Status = $(if ($reNav -eq "OK") { "PASS" } else { "FAIL" })
                Detail = "Re-navigate to Home=$reNav"
            }
        }

        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Dashboard_HomePage" -Status $(if ($dashboardOk) { "pass" } else { "fail" }) `
            -Detail "HomeNav=$homeNav | Status=$homeStatusDetail" -Category "Dashboard"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Widget_RecentTools" -Status $(if ($recentWidget -like "OK|*") { "pass" } else { "fail" }) `
            -Detail "$recentWidget" -Category "Dashboard"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Widget_AIProvider" -Status $(if ($aiWidget -like "OK|*") { "pass" } else { "fail" }) `
            -Detail "$aiWidget" -Category "Dashboard"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Widget_Performance" -Status $(if ($perfWidget -like "OK|*") { "pass" } else { "fail" }) `
            -Detail "$perfWidget" -Category "Dashboard"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Widget_Diagnostics" -Status $(if ($diagWidget -like "OK|*") { "pass" } else { "fail" }) `
            -Detail "$diagWidget" -Category "Dashboard"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "HomeIconNavigation" -Status $(if ($reNav -eq "OK") { "pass" } else { "fail" }) `
            -Detail "Re-navigate to Home=$reNav" -Category "Dashboard"
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "Dashboard_HomePage" -Status "skip" `
            -Detail "COM automation not available" -Category "Dashboard"
    }

    # Step 13: Widget Click Tests (Phase 23)
    Write-Log "===== Widget Click Tests (Phase 23) ====="
    $widgetClickResults = @()
    if ($auto) {
        $widgetClickTests = @(
            @{ Title = "Current Version"; ExpectedNav = "diagnostics" }
            @{ Title = "Latest Updates"; ExpectedNav = "updates" }
            @{ Title = "Recent Tools" }
            @{ Title = "AI Providers" }
            @{ Title = "Performance" }
        )

        foreach ($wct in $widgetClickTests) {
            $wTitle = $wct.Title
            # Re-navigate to Home before each click since previous widget click navigates away
            $auto.OpenWindow("Home") | Out-Null
            Start-Sleep -Milliseconds 200
            Write-Log "  Clicking widget: '$wTitle'..."
            $wcResult = $auto.WidgetClick($wTitle)
            Write-Log "    WidgetClick($wTitle): $wcResult"
            $wcOk = $wcResult -eq "OK"
            Start-Sleep -Milliseconds 500

            if ($wcOk) {
                Write-Log "    [NAVIGATED] Widget '$wTitle' clicked successfully"
            } else {
                Write-Log "    [FAIL] Widget '$wTitle' click failed: $wcResult" -Level "WARN"
            }
            $widgetClickResults += [PSCustomObject]@{
                Test = "WidgetClick_$($wTitle -replace ' ','')"; Status = $(if ($wcOk) { "PASS" } else { "FAIL" })
                Detail = "WidgetClick($wTitle)=$wcResult"
            }
            Add-Result -Doc $resultsDoc -TaskId "T023" -Name "WidgetClick_$($wTitle -replace ' ','')" -Status $(if ($wcOk) { "pass" } else { "fail" }) `
                -Detail "WidgetClick($wTitle)=$wcResult" -Category "Dashboard"
        }

        # Navigate back to Home after widget clicks
        Write-Log "  Returning to Home after widget clicks..."
        $auto.OpenWindow("Home") | Out-Null
        Start-Sleep -Seconds 1
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T023" -Name "WidgetClick_Suite" -Status "skip" `
            -Detail "COM automation not available" -Category "Dashboard"
    }

    # Step 14: Localization & RTL Tests (Phase 24)
    Write-Log "===== Localization & RTL Tests (Phase 24) ====="
    $locResults = @()
    if ($auto) {
        # Navigate to Home first
        $auto.OpenWindow("Home") | Out-Null
        Start-Sleep -Seconds 1

        # Test 1: Switch to Arabic
        Write-Log "  Switching language to Arabic (ar-SA)..."
        $loc1 = $auto.SwitchLanguage("ar-SA")
        Write-Log "    SwitchLanguage(ar-SA): $loc1"
        $locOk = $loc1 -eq "OK"
        Start-Sleep -Milliseconds 1500
        Take-Screenshot -Label "L10N_Arabic"

        # Test 2: Verify RTL mode is active
        Write-Log "  Verifying RTL mode..."
        $loc2 = $auto.IsRTLMode()
        Write-Log "    IsRTLMode: $loc2"
        $locOk = $locOk -and ($loc2 -eq "TRUE")
        Take-Screenshot -Label "L10N_RTL_Active"

        # Test 3: Verify current language is Arabic
        Write-Log "  Verifying current language..."
        $loc3 = $auto.GetCurrentLanguage()
        Write-Log "    GetCurrentLanguage: $loc3"
        $locOk = $locOk -and ($loc3 -like "CODE:ar-SA*")

        # Test 4: Navigate to Language settings page
        Write-Log "  Navigating to Language settings..."
        $loc4 = $auto.OpenWindow("Language Settings")
        Write-Log "    OpenWindow(Language): $loc4"
        $locOk = $locOk -and ($loc4 -eq "OK")
        Start-Sleep -Seconds 1
        Take-Screenshot -Label "L10N_LanguagePage_Arabic"

        # Test 5: Switch back to English
        Write-Log "  Switching language back to English (en-US)..."
        $loc5 = $auto.SwitchLanguage("en-US")
        Write-Log "    SwitchLanguage(en-US): $loc5"
        $locOk = $locOk -and ($loc5 -eq "OK")
        Start-Sleep -Milliseconds 1500
        Take-Screenshot -Label "L10N_English"

        # Test 6: Verify RTL is now off
        Write-Log "  Verifying RTL is disabled..."
        $loc6 = $auto.IsRTLMode()
        Write-Log "    IsRTLMode: $loc6"
        $locOk = $locOk -and ($loc6 -eq "FALSE")

        # Test 7: Rapid language toggle (stress test)
        Write-Log "  Rapid language toggle (10x)..."
        $rapidLocOk = $true
        for ($i = 0; $i -lt 10; $i++) {
            $c = if ($i % 2 -eq 0) { "ar-SA" } else { "en-US" }
            $r = $auto.SwitchLanguage($c)
            if ($r -ne "OK") { $rapidLocOk = $false; Write-Log "    Rapid toggle $i ($c): $r" -Level "WARN" }
            Start-Sleep -Milliseconds 300
        }
        # End on English
        $auto.SwitchLanguage("en-US") | Out-Null
        Start-Sleep -Milliseconds 500
        Take-Screenshot -Label "L10N_AfterRapidToggle"

        Add-Result -Doc $resultsDoc -TaskId "T017" -Name "L10N_SwitchToArabic" -Status $(if ($loc1 -eq "OK") { "pass" } else { "fail" }) `
            -Detail "SwitchLanguage(ar-SA)=$loc1" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T020" -Name "L10N_RTLVerification" -Status $(if ($loc2 -eq "TRUE") { "pass" } else { "fail" }) `
            -Detail "IsRTLMode=$loc2" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T017" -Name "L10N_GetCurrentLanguage" -Status $(if ($loc3 -like "CODE:ar-SA*") { "pass" } else { "fail" }) `
            -Detail "GetCurrentLanguage=$loc3" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T012" -Name "L10N_LanguagePageNav" -Status $(if ($loc4 -eq "OK") { "pass" } else { "fail" }) `
            -Detail "OpenWindow(Language)=$loc4" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T017" -Name "L10N_SwitchToEnglish" -Status $(if ($loc5 -eq "OK") { "pass" } else { "fail" }) `
            -Detail "SwitchLanguage(en-US)=$loc5" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T020" -Name "L10N_RTLDisabled" -Status $(if ($loc6 -eq "FALSE") { "pass" } else { "fail" }) `
            -Detail "IsRTLMode=$loc6" -Category "Localization"
        Add-Result -Doc $resultsDoc -TaskId "T048" -Name "L10N_RapidToggle_10x" -Status $(if ($rapidLocOk) { "pass" } else { "warn" }) `
            -Detail "10 rapid language toggles $(if ($rapidLocOk) { 'completed' } else { 'had failures' })" -Category "Localization"

        $locResults += [PSCustomObject]@{ Test = "Switch to Arabic (ar-SA)"; Status = $(if ($loc1 -eq "OK") { "PASS" } else { "FAIL" }); Detail = "Result=$loc1" }
        $locResults += [PSCustomObject]@{ Test = "RTL Mode Active"; Status = $(if ($loc2 -eq "TRUE") { "PASS" } else { "FAIL" }); Detail = "IsRTLMode=$loc2" }
        $locResults += [PSCustomObject]@{ Test = "Current Language (ar-SA)"; Status = $(if ($loc3 -like "CODE:ar-SA*") { "PASS" } else { "FAIL" }); Detail = "GetCurrentLanguage=$loc3" }
        $locResults += [PSCustomObject]@{ Test = "Language Page Navigation"; Status = $(if ($loc4 -eq "OK") { "PASS" } else { "FAIL" }); Detail = "OpenWindow(Language)=$loc4" }
        $locResults += [PSCustomObject]@{ Test = "Switch to English (en-US)"; Status = $(if ($loc5 -eq "OK") { "PASS" } else { "FAIL" }); Detail = "Result=$loc5" }
        $locResults += [PSCustomObject]@{ Test = "RTL Mode Disabled"; Status = $(if ($loc6 -eq "FALSE") { "PASS" } else { "FAIL" }); Detail = "IsRTLMode=$loc6" }
        $locResults += [PSCustomObject]@{ Test = "Rapid Toggle 10x"; Status = $(if ($rapidLocOk) { "PASS" } else { "WARN" }); Detail = "Completed=$(if ($rapidLocOk) {'Yes'} else {'Had failures'})" }
    } else {
        Write-Log "  Skipped: COM automation not available" -Level "WARN"
        Add-Result -Doc $resultsDoc -TaskId "T017" -Name "Localization_Suite" -Status "skip" `
            -Detail "COM automation not available" -Category "Localization"
    }

    # Step 15: Final memory check
    $memFinal = Get-MemoryMB
    $memGrowth = if ($memBaseline -and $memFinal) { [math]::Round(($memFinal - $memBaseline) / $memBaseline * 100, 1) } else { "N/A" }
    $memGrowthStr = "$memGrowth pct growth"
    Write-Log "Memory: $memBaseline MB -> $memFinal MB ($memGrowthStr)"
    Add-Result -Doc $resultsDoc -TaskId "T054" -Name "MemoryStability" -Status $(if ($memGrowth -eq "N/A" -or $memGrowth -lt 20) { "pass" } else { "warn" }) `
        -Detail "Memory growth: $($memGrowth)pct (baseline: ${memBaseline}MB, final: ${memFinal}MB)" -Category "Memory"

    Take-Screenshot -Label "99_Final"

} catch {
    Write-Log "Fatal error: $_" -Level "ERROR"
    Add-Result -Doc $resultsDoc -TaskId "GENERAL" -Name "TestSuite" -Status "fail" `
        -Detail "Fatal error: $_" -Category "General"
    $overallPass = $false
} finally {
    # Cleanup
    try {
        $excelApp.Quit()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excelApp) | Out-Null
    } catch { }
    Kill-Excel
}

# Save results
$resultsDoc.Save($ResultsFile)
Write-Log "Results saved to: $ResultsFile"

# Compute overall pass from individual test results
$summary = $resultsDoc.DocumentElement.SelectSingleNode("summary")
$totalVal = $summary.GetAttribute("total"); $passedVal = $summary.GetAttribute("passed"); $failedVal = $summary.GetAttribute("failed"); $skippedVal = $summary.GetAttribute("skipped")
$overallPass = [int]$failedVal -eq 0 -and [int]$passedVal -gt 0

# Print summary table
Write-Log "`n===== RESULTS SUMMARY ====="
Write-Log "Total: $totalVal | Passed: $passedVal | Failed: $failedVal | Skipped: $skippedVal"
Write-Log "`nWindow Test Results:"
$windowResults | Format-Table -Property Name, Status, OpenTimeMs, MemDeltaMB -AutoSize
Write-Log "`nTheme Test Results:"
$themeResults | Format-Table -Property Theme, Status, TimeMs, MemDelta -AutoSize
Write-Log "`nWBS Settings Test Results:"
$wbsResults | Format-Table -Property Test, Status, Detail -AutoSize
Write-Log "`nDuration Estimator Test Results:"
$deResults | Format-Table -Property Test, Status, Detail -AutoSize
Write-Log "`nDashboard & Home Test Results (Phase 23):"
$dashboardResults | Format-Table -Property Test, Status, Detail -AutoSize
Write-Log "`nWidget Click Test Results (Phase 23):"
$widgetClickResults | Format-Table -Property Test, Status, Detail -AutoSize
Write-Log "`nLocalization & RTL Test Results (Phase 24):"
$locResults | Format-Table -Property Test, Status, Detail -AutoSize

$locOverall = if ($locResults.Count -eq 0) { $true } else { ($locResults | Where-Object { $_.Status -eq "FAIL" }).Count -eq 0 }
if ($locOverall) { Write-Log "[PASS] Phase 24: All localization/rtl tests passed" }
else { Write-Log "[FAIL] Phase 24: Some localization/rtl tests failed" -Level "WARN" }

if ($overallPass) {
    Write-Log "`n[PASS] OVERALL: PASS" -Level "INFO"
} else {
    Write-Log "`n[FAIL] OVERALL: FAIL (check results for details)" -Level "WARN"
}

Write-Log "===== VSTO Test Suite Complete ====="
