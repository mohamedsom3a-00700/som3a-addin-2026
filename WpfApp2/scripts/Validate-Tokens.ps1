param(
    [string]$ProjectRoot = "..",
    [switch]$Quiet
)

$errors = @()
$warnings = @()

function Write-ErrorMsg {
    param([string]$Msg)
    $errors += $Msg
    if (-not $Quiet) { Write-Host "  FAIL: $Msg" -ForegroundColor Red }
}

function Write-PassMsg {
    param([string]$Msg)
    if (-not $Quiet) { Write-Host "  PASS: $Msg" -ForegroundColor Green }
}

$resolvedRoot = [System.IO.Path]::GetFullPath((Join-Path -Path (Get-Location) -ChildPath $ProjectRoot))

# ---------------------------------------------------------------------------
# 1. Check that all required XAML resource files exist
# ---------------------------------------------------------------------------
Write-Host "`n[1/5] Checking required resource files..." -ForegroundColor Cyan

$requiredFiles = @(
    "Theme/Base/Colors.xaml", "Theme/Base/Typography.xaml", "Theme/Base/Spacing.xaml",
    "Theme/Base/Radius.xaml", "Theme/Base/Elevation.xaml", "Theme/Base/Motion.xaml",
    "Theme/Base/ZIndex.xaml", "Theme/Base/Opacity.xaml", "Theme/Base/ComponentTokens.xaml",
    "Theme/Effects/Shadows.xaml", "Theme/Effects/Glow.xaml", "Theme/Effects/Animations.xaml",
    "Theme/Controls/ButtonStyles.xaml", "Theme/Controls/TextBoxStyles.xaml",
    "Theme/Controls/ComboBoxStyles.xaml", "Theme/Controls/CheckBoxStyles.xaml",
    "Theme/Controls/RadioButtonStyles.xaml", "Theme/Controls/ToggleButtonStyles.xaml",
    "Theme/Controls/DataGridStyles.xaml", "Theme/Controls/ScrollViewerStyles.xaml",
    "Theme/Controls/ScrollBarStyles.xaml", "Theme/Controls/ThemeCardStyles.xaml",
    "Theme/Controls/AccentSwatchStyles.xaml", "Theme/Controls/WindowStyles.xaml",
    "Theme/Controls/WindowButtonStyles.xaml", "Theme/Controls/GroupBoxStyles.xaml",
    "Theme/Controls/LabelStyles.xaml", "Theme/Controls/ListViewStyles.xaml",
    "Theme/Controls/ListViewItemStyles.xaml", "Theme/Controls/ComboBoxItemStyles.xaml",
    "Theme/Controls/ProgressBarStyles.xaml",
    "Theme/ModernWindow.xaml", "Theme/WindowAnimations.xaml", "Theme/ThemeResources.xaml",
    "Theme/Dark/DarkColors.xaml", "Theme/Dark/DarkTheme.xaml",
    "Theme/Light/LightColors.xaml", "Theme/Light/LightTheme.xaml",
    "Theme/Custom/CustomColors.xaml", "Theme/Custom/CustomTheme.xaml",
    "Services/ThemeManager.cs", "Services/RenderModeService.cs", "Services/WindowRenderModeDetector.cs"
)

$allFound = $true
foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $resolvedRoot $file))) {
        Write-ErrorMsg "Missing required file: $file"
        $allFound = $false
    }
}
if ($allFound) { Write-PassMsg "All $($requiredFiles.Count) required resource files exist" }

# ---------------------------------------------------------------------------
# 2. Check required token keys in XAML files
# ---------------------------------------------------------------------------
Write-Host "`n[2/5] Checking required token definitions..." -ForegroundColor Cyan

function Test-Token {
    param([string]$FilePath, [string]$TokenKey)
    $fullPath = Join-Path $resolvedRoot $FilePath
    if (-not (Test-Path $fullPath)) { return $false }
    return (Get-Content $fullPath -Raw) -match [regex]::Escape("x:Key=""$TokenKey""")
}

$checks = @(
    @{File="Theme/Base/Colors.xaml"; Tokens=@(
        "Primitive.Blue.500","Primitive.Slate.900","Primitive.White.95",
        "Brush.Background.Primary","Brush.Text.Primary","Brush.Accent.Primary",
        "Brush.Control.Background","Brush.Button.Background",
        "Brush.ScrollBar.Thumb","Brush.GroupBox.Background","Brush.Overlay.Background",
        "Brush.Background.Root","Brush.Accent.ProgressFill",
        "Brush.Stroke.Info","Brush.Stroke.Status","Brush.Fill.Info","Brush.Fill.Status",
        "AccentBrush"
    )},
    @{File="Theme/Base/Elevation.xaml"; Tokens=@(
        "Elevation.Card.OffsetX","Elevation.Card.OffsetY","Elevation.Card.BlurRadius","Elevation.Card.Opacity",
        "Elevation.Popup.OffsetX","Elevation.Popup.OffsetY","Elevation.Popup.BlurRadius","Elevation.Popup.Opacity",
        "Elevation.Window.OffsetX","Elevation.Window.OffsetY","Elevation.Window.BlurRadius","Elevation.Window.Opacity"
    )},
    @{File="Theme/Effects/Shadows.xaml"; Tokens=@(
        "Shadow.Window","Shadow.Popup","Shadow.Popup.Small","Shadow.Card",
        "Shadow.Small","Shadow.Medium","Shadow.Large",
        "Shadow.Window.Safe","Shadow.Card.Safe","Shadow.Popup.Safe",
        "ProgressGlow","Elevation.Card","Elevation.Popup","Elevation.Window"
    )}
)

$allTokensFound = $true
foreach ($c in $checks) {
    foreach ($t in $c.Tokens) {
        if (-not (Test-Token -FilePath $c.File -TokenKey $t)) {
            Write-ErrorMsg "Missing token '$t' in $($c.File)"
            $allTokensFound = $false
        }
    }
}
if ($allTokensFound) { Write-PassMsg "All required tokens found in token definition files" }

$runtimeOk = $true
foreach ($t in @("AccentColorBrush","AccentColorValue","AccentColor")) {
    if (Test-Token -FilePath "Theme/Base/Colors.xaml" -TokenKey $t) {
        Write-ErrorMsg "Runtime token '$t' must NOT be hardcoded in Colors.xaml (set by ThemeManager)"
        $runtimeOk = $false
    }
}
if ($runtimeOk) { Write-PassMsg "Runtime tokens correctly managed by ThemeManager (not in XAML)" }

# ---------------------------------------------------------------------------
# 3. Check for hardcoded #HEX colors in control/window XAML only
#    Whitelist: Base/, Dark/, Light/, Custom/, Effects/, obj/, bin/
#    Skip hex in Setter Value="..." (data values like swatch definitions)
# ---------------------------------------------------------------------------
Write-Host "`n[3/5] Checking for hardcoded #HEX colors in control/window XAML..." -ForegroundColor Cyan

$excludeDirSuffixes = @(
    "\Base", "\Dark", "\Light", "\Custom", "\Effects", "\obj", "\bin"
)

function Is-ExcludedDir {
    param([string]$Dir)
    foreach ($suffix in $excludeDirSuffixes) {
        if ($Dir.EndsWith($suffix) -or $Dir -like "*$suffix\*") { return $true }
    }
    return $false
}

$targetXamlFiles = Get-ChildItem $resolvedRoot -Recurse -Filter "*.xaml" | Where-Object {
    -not (Is-ExcludedDir $_.DirectoryName) -and
    $_.Name -ne "ThemeResources.xaml"
}

$hits = @{}
foreach ($f in $targetXamlFiles) {
    $lines = Get-Content $f.FullName
    $lineHits = @()
    for ($ln = 0; $ln -lt $lines.Count; $ln++) {
        $line = $lines[$ln]
        # Skip lines that are Setter Value definitions (data values like swatch hex codes)
        if ($line -match 'Setter\s+Property\s*=\s*"Tag"\s+Value\s*=\s*"#[0-9A-Fa-f]+"') { continue }
        $hexMatch = [regex]::Match($line, '#[0-9A-Fa-f]{6}(?:[0-9A-Fa-f]{2})?')
        if ($hexMatch.Success) {
            $lineHits += "$($hexMatch.Value) (line $($ln+1))"
        }
    }
    if ($lineHits.Count -gt 0) {
        $rel = $f.FullName.Substring($resolvedRoot.Length).TrimStart('\')
        $hits[$rel] = $lineHits
    }
}

if ($hits.Count -eq 0) {
    Write-PassMsg "No hardcoded #HEX colors found in control/window XAML files"
} else {
    foreach ($kv in $hits.GetEnumerator()) {
        foreach ($hit in $kv.Value) {
            Write-ErrorMsg "Hardcoded HEX $hit in $($kv.Key)"
        }
    }
}

# ---------------------------------------------------------------------------
# 4. Check for inline DropShadowEffect outside Effects/
# ---------------------------------------------------------------------------
Write-Host "`n[4/5] Checking for inline DropShadowEffect outside Effects/..." -ForegroundColor Cyan

$nonEffectXaml = Get-ChildItem $resolvedRoot -Recurse -Filter "*.xaml" | Where-Object {
    -not (Is-ExcludedDir $_.DirectoryName) -and
    $_.Name -ne "ThemeResources.xaml"
}

$effectHits = @{}
foreach ($f in $nonEffectXaml) {
    $content = Get-Content $f.FullName -Raw
    if ($content -match '<DropShadowEffect') {
        $rel = $f.FullName.Substring($resolvedRoot.Length).TrimStart('\')
        $effectHits[$rel] = $true
    }
}

if ($effectHits.Count -eq 0) {
    Write-PassMsg "No inline DropShadowEffect found outside Effects/ directory"
} else {
    foreach ($k in $effectHits.Keys) {
        Write-ErrorMsg "Inline DropShadowEffect found in: $k"
    }
}

# ---------------------------------------------------------------------------
# 5. Validate Resource Loading Order in ThemeResources.xaml
# ---------------------------------------------------------------------------
Write-Host "`n[5/5] Validating resource loading order in ThemeResources.xaml..." -ForegroundColor Cyan

$trPath = Join-Path $resolvedRoot "Theme/ThemeResources.xaml"
if (Test-Path $trPath) {
    $content = Get-Content $trPath -Raw

    $expected = @(
        "Theme/Base/Colors.xaml",
        "Theme/Base/Typography.xaml",
        "Theme/Base/Spacing.xaml",
        "Theme/Base/Radius.xaml",
        "Theme/Base/Elevation.xaml",
        "Theme/Base/Motion.xaml",
        "Theme/Base/ZIndex.xaml",
        "Theme/Base/Opacity.xaml",
        "Theme/Base/ComponentTokens.xaml",
        "Theme/Effects/Shadows.xaml",
        "Theme/Effects/Glow.xaml",
        "Theme/Effects/Animations.xaml",
        "Theme/Controls/ButtonStyles.xaml",
        "Theme/Controls/AccentSwatchStyles.xaml",
        "Theme/Controls/ThemeCardStyles.xaml",
        "Theme/Controls/TextBoxStyles.xaml",
        "Theme/Controls/ComboBoxStyles.xaml",
        "Theme/Controls/DataGridStyles.xaml",
        "Theme/Controls/ListViewStyles.xaml",
        "Theme/Controls/ScrollBarStyles.xaml",
        "Theme/Controls/CheckBoxStyles.xaml",
        "Theme/Controls/RadioButtonStyles.xaml",
        "Theme/Controls/ToggleButtonStyles.xaml",
        "Theme/Controls/ScrollViewerStyles.xaml",
        "Theme/Controls/WindowStyles.xaml",
        "Theme/Controls/GroupBoxStyles.xaml",
        "Theme/Controls/LabelStyles.xaml",
        "Theme/Controls/ListViewItemStyles.xaml",
        "Theme/Controls/ComboBoxItemStyles.xaml",
        "Theme/Controls/ProgressBarStyles.xaml",
        "Theme/Controls/WindowButtonStyles.xaml",
        "Theme/ModernWindow.xaml",
        "Theme/WindowAnimations.xaml"
    )

    $actual = [regex]::Matches($content, 'component/([^"]+\.xaml)"') | ForEach-Object { $_.Groups[1].Value }

    $ok = $true
    for ($i = 0; $i -lt $expected.Count; $i++) {
        if ($i -ge $actual.Count) {
            Write-ErrorMsg "Missing entry: expected '$($expected[$i])' but no more entries found"
            $ok = $false
            break
        }
        if ($actual[$i] -ne $expected[$i]) {
            Write-ErrorMsg "Loading order violation at position $($i+1): expected '$($expected[$i])' but found '$($actual[$i])'"
            $ok = $false
        }
    }
    if ($ok) { Write-PassMsg "Resource loading order in ThemeResources.xaml is correct" }
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Token Validation Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Errors: $($errors.Count)" -ForegroundColor $(if ($errors.Count -gt 0) { "Red" } else { "Green" })

if ($errors.Count -gt 0) {
    Write-Host "`n❌ VALIDATION FAILED - Fix errors above and re-run" -ForegroundColor Red
    exit 1
} else {
    Write-Host "`n✅ VALIDATION PASSED - All checks OK" -ForegroundColor Green
    exit 0
}
