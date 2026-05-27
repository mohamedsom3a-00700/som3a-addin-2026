<#
.SYNOPSIS
    Debug script: dump all UIA elements from BOQ page
#>

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

$Win32Code = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public class Win32 {
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    public static uint GetProcessId(IntPtr hWnd) { GetWindowThreadProcessId(hWnd, out uint pid); return pid; }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsWindowVisible(IntPtr hWnd);
}
'@
Add-Type -TypeDefinition $Win32Code -Language CSharp

function Get-ChildHwnds {
    param([IntPtr]$Parent)
    $result = @()
    $code = @'
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
public class EnumWindows {
    public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowProc lpEnumFunc, IntPtr lParam);
    public static List<IntPtr> EnumChildren(IntPtr parent) {
        var list = new List<IntPtr>();
        EnumChildWindows(parent, (hwnd, lp) => { list.Add(hwnd); return true; }, IntPtr.Zero);
        return list;
    }
}
'@
    Add-Type -TypeDefinition $code -Language CSharp -ErrorAction SilentlyContinue
    return [EnumWindows]::EnumChildren($Parent)
}

Write-Host "===== UIA DUMP =====" -ForegroundColor Yellow

# Kill old Excel
Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 2

$excelPath = "C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE"
$testFile = "$PSScriptRoot\Book2.xlsx"

$proc = Start-Process -FilePath $excelPath -ArgumentList "`"$testFile`"" -PassThru
Start-Sleep 10

$excel = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Excel.Application")
$excel.Visible = $true
$addin = $excel.COMAddIns | Where-Object { $_.Description -like "*Som3a Addin 2026*" }
if (-not $addin) { Write-Host "[WARN] Add-in not found, skipping BOQ navigation" -ForegroundColor Yellow }
else {
    $auto = $addin.Object
    if ($auto) { $auto.OpenWindow("BOQ Activity Generator"); Start-Sleep 5 }
    else { Write-Host "[WARN] addin.Object is null" -ForegroundColor Yellow }
}

Write-Host "`n=== Excel child HWNDs ===" -ForegroundColor Cyan
$excelProc = Get-Process -Id $proc.Id
$mainHwnd = $excelProc.MainWindowHandle
Write-Host "Main HWND: $mainHwnd"
$kids = Get-ChildHwnds -Parent $mainHwnd
foreach ($hw in $kids) {
    $sb = New-Object System.Text.StringBuilder 256
    [Win32]::GetWindowText($hw, $sb, $sb.Capacity)
    $text = $sb.ToString()
    if ($text) { Write-Host "  $hw : '$text'" }
}

Write-Host "`n=== UIA: All buttons on desktop ===" -ForegroundColor Cyan
$root = [System.Windows.Automation.AutomationElement]::RootElement
$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$allBtns = $root.FindAll([System.Windows.Automation.TreeScope]::Subtree, $btnCond)
Write-Host "Total buttons: $($allBtns.Count)"
foreach ($b in $allBtns) {
    try {
        $n = $b.Current.Name
        $aid = $b.Current.AutomationId
        $cls = $b.Current.ClassName
        $frm = $b.Current.FrameworkId
        if ($n -or $aid) {
            Write-Host "  Name='$n' Aid='$aid' Class='$cls' Frame='$frm'"
        }
    } catch {}
}

Write-Host "`n=== UIA: All edit fields ===" -ForegroundColor Cyan
$editCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Edit)
$allEdits = $root.FindAll([System.Windows.Automation.TreeScope]::Subtree, $editCond)
Write-Host "Total edits: $($allEdits.Count)"
foreach ($e in $allEdits) {
    try {
        $n = $e.Current.Name
        $aid = $e.Current.AutomationId
        if ($n -or $aid) { Write-Host "  Name='$n' Aid='$aid'" }
    } catch {}
}

Write-Host "`n=== UIA: text 'Load' 'BOQ' anywhere ===" -ForegroundColor Cyan
$all = $root.FindAll([System.Windows.Automation.TreeScope]::Subtree, [System.Windows.Automation.Condition]::TrueCondition)
Write-Host "Total elements: $($all.Count)"
foreach ($e in $all) {
    try {
        $n = $e.Current.Name
        $ct = $e.Current.ControlType.ProgrammaticName
        $aid = $e.Current.AutomationId
        $cls = $e.Current.ClassName
        $frm = $e.Current.FrameworkId
        if ($n -match "Load|BOQ|boq|Load|Generate|Consent|Activities|activity") {
            Write-Host "  Name='$n' Ctrl=$ct Aid='$aid' Cls='$cls' Frame='$frm'"
        }
    } catch {}
}

# Cleanup
$excel.Quit()
Start-Sleep 2
Get-Process EXCEL -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "`nDone" -ForegroundColor Green
