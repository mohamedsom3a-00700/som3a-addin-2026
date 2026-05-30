#!/usr/bin/env pwsh
# Automated verification script for CommunityToolkit.Mvvm migration
# Scans ViewModels for forbidden manual MVVM patterns

[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [switch]$Fix
)

$ErrorView = 'NormalView'
$exitCode = 0
$viewModelsDir = Join-Path $RepoRoot "WpfApp2\ViewModels"
$modelsDir = Join-Path $RepoRoot "WpfApp2\Models"
$helpersDir = Join-Path $RepoRoot "WpfApp2\Helpers"

$issues = @()

function Report-Issue {
    param([string]$File, [int]$Line, [string]$Message)
    $script:issues += [PSCustomObject]@{ File = $File; Line = $Line; Message = $Message }
}

# --- Check 1: Non-partial ViewModel classes ---
$vmFiles = Get-ChildItem -Path $viewModelsDir -Filter "*.cs" -Recurse
foreach ($file in $vmFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $matches = [regex]::Matches($content, '(?<!\bpartial\s+)\bclass\s+(\w+ViewModel)\s*:\s*ViewModelBase')
    foreach ($m in $matches) {
        Report-Issue -File $file.Name -Line ($content.Substring(0, $m.Index).Split("`n").Count) -Message "Non-partial ViewModel: $($m.Groups[1].Value)"
    }
}

# --- Check 2: Manual SetProperty(ref ...) in ViewModels (the real forbidden pattern) ---
foreach ($file in $vmFiles) {
    $lines = Get-Content -Path $file.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match 'SetProperty\s*\(\s*ref\s+') {
            Report-Issue -File $file.Name -Line ($i + 1) -Message "Manual SetProperty(ref ...) found - use [ObservableProperty] instead"
        }
    }
}

# --- Check 3: Old custom RelayCommand/AsyncRelayCommand from Som3a_WPF_UI.Models or Helpers ---
$allCsFiles = Get-ChildItem -Path (Join-Path $RepoRoot "WpfApp2") -Filter "*.cs" -Recurse
foreach ($file in $allCsFiles) {
    $lines = Get-Content -Path $file.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        # Flag old RelayCommand class instantiation (not CommunityToolkit)
        if ($line -match 'new\s+Som3a_WPF_UI\.Models\.RelayCommand\s*\(') {
            Report-Issue -File $file.FullName.Replace($RepoRoot, "").TrimStart("\", "/") -Line ($i + 1) -Message "Old Som3a_WPF_UI.Models.RelayCommand instantiation"
        }
        if ($line -match 'new\s+Som3a_WPF_UI\.Helpers\.AsyncRelayCommand\s*\(') {
            Report-Issue -File $file.FullName.Replace($RepoRoot, "").TrimStart("\", "/") -Line ($i + 1) -Message "Old Som3a_WPF_UI.Helpers.AsyncRelayCommand instantiation"
        }
    }
}

# --- Check 4: Obsolete helper files still exist ---
$obsoleteFiles = @(
    (Join-Path $modelsDir "RelayCommand.cs"),
    (Join-Path $helpersDir "AsyncRelayCommand.cs")
)
foreach ($f in $obsoleteFiles) {
    if (Test-Path $f) {
        Report-Issue -File ($f.Replace($RepoRoot, "").TrimStart("\", "/")) -Line 1 -Message "Obsolete file still exists"
    }
}

# --- Report ---
if ($issues.Count -eq 0) {
    Write-Host "PASS: No forbidden manual MVVM patterns found." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "FAIL: Found $($issues.Count) manual MVVM pattern(s):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "   $($issue.File):$($issue.Line) -- $($issue.Message)" -ForegroundColor Yellow
    }
    exit 1
}
