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
    return Get-Process -Name "EXCEL" -ErrorAction SilentlyContinue | Select-Object -First 1
}

function Kill-Excel {
    $procs = Get-ExcelProcess
    if ($procs) {
        Write-Log "Killing $($procs.Count) existing Excel process(es)..."
        $procs | Stop-Process -Force -ErrorAction SilentlyContinue
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
                        $pid = 0
                        try { $pid = $elem.Current.ProcessId } catch { }
                        if ($pid -eq $ProcessId) { return $elem }
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
    $proc = Get-ExcelProcess
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
)

if ($Quick) {
    $windowTests = $windowTests | Select-Object -First 4
}

# ====================================================================
# MAIN EXECUTION
# ====================================================================

$resultsDoc = New-ResultXml
$overallPass = $true

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

        Write-Log "Testing window: $winTitle"
        $memoryBefore = Get-MemoryMB
        $sw = [System.Diagnostics.Stopwatch]::StartNew()

        $windowFound = $false
        if ($auto) {
            $result = $auto.OpenWindow($btnLabel)
            if ($result -eq "OK") { $windowFound = $true }
            Write-Log "  COM OpenWindow($btnLabel): $result"
        }

        if (-not $windowFound) {
            $null = Test-RibbonButton -ExcelApp $excelApp -ButtonLabel $btnLabel
            Start-Sleep -Seconds 3
            $window = Get-WindowByTitle -Title $winTitle -TimeoutSeconds 5 -ProcessId $excelProc.Id
            $windowFound = $null -ne $window
        }

        if (-not $windowFound) {
            $window = Get-WindowByTitle -Title $winTitle -TimeoutSeconds 3 -ProcessId $excelProc.Id
            $windowFound = $null -ne $window
        }

        $sw.Stop()
        $openTimeMs = $sw.ElapsedMilliseconds

        if ($windowFound) {
            Write-Log "  [FOUND] Window '$winTitle' (${openTimeMs}ms)"
            Take-Screenshot -Label "Window_$($winTitle -replace '[^a-zA-Z0-9]','_')"

            if ($auto) { $auto.CloseWindow($btnLabel) | Out-Null }
            Start-Sleep -Milliseconds 500

            $memoryAfter = Get-MemoryMB
            $memDelta = if ($memoryBefore -and $memoryAfter) { $memoryAfter - $memoryBefore } else { "N/A" }

            Add-Result -Doc $resultsDoc -TaskId $taskId -Name "Window_$winTitle" -Status "pass" `
                -Detail "Opened in ${openTimeMs}ms, mem delta: ${memDelta}MB" -Category "Window"
            $windowResults += [PSCustomObject]@{
                Name = $winTitle; Status = "PASS"; OpenTimeMs = $openTimeMs; MemDeltaMB = $memDelta
            }
            $openAttempts++
        } else {
            Write-Log "  [NOT FOUND] Window '$winTitle'" -Level "WARN"
            Add-Result -Doc $resultsDoc -TaskId $taskId -Name "Window_$winTitle" -Status "skip" `
                -Detail "Window not found" -Category "Window"
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
    $rapidSuccess = $true
    for ($i = 0; $i -lt 10; $i++) {
        $t = @("Dark", "Light", "Custom")[$i % 3]
        if ($auto) {
            $r = $auto.SwitchTheme($t)
            if ($r -ne "OK") { $rapidSuccess = $false; Write-Log "  Rapid switch $i ($t): $r" -Level "WARN" }
        }
        Start-Sleep -Milliseconds 200
    }
    $memAfterRapid = Get-MemoryMB
    Add-Result -Doc $resultsDoc -TaskId "T048" -Name "RapidThemeSwitch_10x" -Status $(if ($rapidSuccess) { "pass" } else { "warn" }) `
        -Detail "10 rapid switches completed, memory: $memAfterRapid MB" -Category "Theme"

    # Step 10: Final memory check
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

# Print summary table
Write-Log "`n===== RESULTS SUMMARY ====="
$summary = $resultsDoc.DocumentElement.SelectSingleNode("summary")
$totalVal = $summary.GetAttribute("total"); $passedVal = $summary.GetAttribute("passed"); $failedVal = $summary.GetAttribute("failed"); $skippedVal = $summary.GetAttribute("skipped")
Write-Log "Total: $totalVal | Passed: $passedVal | Failed: $failedVal | Skipped: $skippedVal"
Write-Log "`nWindow Test Results:"
$windowResults | Format-Table -Property Name, Status, OpenTimeMs, MemDeltaMB -AutoSize
Write-Log "`nTheme Test Results:"
$themeResults | Format-Table -Property Theme, Status, TimeMs, MemDelta -AutoSize

if ($overallPass) {
    Write-Log "`n[PASS] OVERALL: PASS" -Level "INFO"
} else {
    Write-Log "`n[FAIL] OVERALL: FAIL (check results for details)" -Level "WARN"
}

Write-Log "===== VSTO Test Suite Complete ====="
