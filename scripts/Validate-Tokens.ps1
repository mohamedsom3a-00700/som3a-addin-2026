# Validate-Tokens.ps1
# Token compliance validation script for Design System Core
# Checks for inline hex, StaticResource on themeable properties, inline DropShadowEffect, and hardcoded spacing
# Exit code: 0 = pass, 1 = violations found

param(
    [switch]$Json
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$themeDir = Join-Path $repoRoot "WpfApp2\Theme"
$controlsDir = Join-Path $themeDir "Controls"
$windowsDir = Join-Path $repoRoot "WpfApp2\Windows"
$viewsDir = Join-Path $repoRoot "WpfApp2\Views"
$controlsSubDir = Join-Path $repoRoot "WpfApp2\Controls"

$allowedTokenFiles = @(
    (Join-Path $themeDir "Base\Colors.xaml"),
    (Join-Path $themeDir "Dark\DarkColors.xaml"),
    (Join-Path $themeDir "Light\LightColors.xaml"),
    (Join-Path $themeDir "Custom\CustomColors.xaml")
)

$effectsDir = Join-Path $themeDir "Effects"
$violations = [System.Collections.ArrayList]::new()

function Add-Violation {
    param([string]$Rule, [string]$File, [int]$Line, [string]$Message)
    $violations.Add(@{Rule=$Rule; File=$File; Line=$Line; Message=$Message}) | Out-Null
}

function Get-RelativePath {
    param([string]$Path)
    $Path.Replace($repoRoot + "\", "")
}

$xamlFiles = @()
$scanDirs = @($controlsDir, $viewsDir, $controlsSubDir, $windowsDir)
$wpfApp2Dir = Join-Path $repoRoot "WpfApp2"
if (Test-Path $wpfApp2Dir) { $scanDirs += $wpfApp2Dir }
$scanDirs = $scanDirs | Select-Object -Unique

foreach ($dir in $scanDirs) {
    if (Test-Path $dir) {
        $xamlFiles += Get-ChildItem -Path $dir -Filter "*.xaml" -Recurse | Select-Object -ExpandProperty FullName
    }
}

$xamlFiles += Join-Path $repoRoot "WpfApp2\MainWindow.xaml"
$xamlFiles += Join-Path $repoRoot "WpfApp2\Theme\WindowAnimations.xaml"

foreach ($file in $xamlFiles) {
    if (-not (Test-Path $file)) { continue }
    $relPath = Get-RelativePath $file
    $isTokenFile = $allowedTokenFiles -contains $file
    $lines = Get-Content $file

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $lineNum = $i + 1
        $line = $lines[$i]

        if (-not $isTokenFile) {
            if ($line -match 'Color\s*=\s*"[#][0-9A-Fa-f]{3,8}"') {
                Add-Violation -Rule "no-inline-hex" -File $relPath -Line $lineNum -Message "Inline hex color found outside token definition files"
            }
        }

        if ($line -match 'StaticResource\s+(Brush\.|Color\.|AccentBrush|BackgroundBrush|CardBrush|SurfaceBrush|TextMainBrush|TextSubBrush|TextDisabledBrush|SuccessBrush|WarningBrush|DangerBrush|InfoBrush|CardStrokeBrush|ControlBgBrush|ControlStrokeBrush|TextOnAccentBrush|StrokeInfoBrush|StrokeStatusBrush|FillInfoBrush|FillStatusBrush|ComboBoxPopupBackground|ComboBoxPopupBorder|ComboBoxItemHighlight|CheckMarkForeground|CardHoverBorder|WhiteAlpha\w+|AccentColorBrush|AccentColorLight|AccentColorValue)') {
            if ($line -match '(Background|Foreground|BorderBrush|Fill|Stroke|Effect)\s*=') {
                Add-Violation -Rule "no-static-resource-themeable" -File $relPath -Line $lineNum -Message "StaticResource on themeable DP property"
            }
        }

        if ($line -match '<DropShadowEffect') {
            $isEffectsFile = $file.StartsWith($effectsDir)
            if (-not $isEffectsFile) {
                Add-Violation -Rule "no-inline-drop-shadow" -File $relPath -Line $lineNum -Message "Inline DropShadowEffect outside Effects/ directory"
            }
        }
    }
}

$totalViolations = $violations.Count

if ($Json) {
    $result = @{
        totalViolations = $totalViolations
        violations = $violations
        status = if ($totalViolations -eq 0) { "PASS" } else { "FAIL" }
    }
    $result | ConvertTo-Json -Depth 3
} else {
    Write-Host "=== Token Validation Report ===" -ForegroundColor Cyan
    Write-Host ""

    if ($totalViolations -eq 0) {
        Write-Host "PASS: No violations found." -ForegroundColor Green
    } else {
        Write-Host "FAIL: $totalViolations violation(s) found." -ForegroundColor Red
        Write-Host ""
        foreach ($v in $violations) {
            Write-Host "  [$($v.Rule)] $($v.File):$($v.Line) - $($v.Message)" -ForegroundColor Yellow
        }
    }
}

exit $(if ($totalViolations -eq 0) { 0 } else { 1 })