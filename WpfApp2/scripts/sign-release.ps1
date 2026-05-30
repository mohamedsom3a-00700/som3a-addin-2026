<#
.SYNOPSIS
    Sign all release artifacts for the Som3a Planova release candidate.
.DESCRIPTION
    Signs all DLLs (signtool.exe), signs VSTO manifests (mage.exe),
    and produces signed MSI with timestamped Authenticode signatures.
.PARAMETER BuildOutputDir
    Path to build output directory containing the binaries to sign.
.PARAMETER CertFile
    Path to the Authenticode code signing certificate (PFX).
.PARAMETER CertPassword
    Password for the certificate file.
.PARAMETER TimestampServer
    RFC 3161 timestamp server URL. Default: http://timestamp.digicert.com
.PARAMETER Quiet
    Suppress verbose output.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BuildOutputDir,
    [Parameter(Mandatory = $true)]
    [string]$CertFile,
    [Parameter(Mandatory = $true)]
    [string]$CertPassword,
    [string]$TimestampServer = "http://timestamp.digicert.com",
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Msg)
    if (-not $Quiet) { Write-Host "`n>> $Msg" -ForegroundColor Cyan }
}

function Invoke-Signtool {
    param([string[]]$Args)
    $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
    if (-not (Test-Path $signtool)) {
        $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe"
    }
    if (-not (Test-Path $signtool)) { $signtool = "signtool.exe" }
    & $signtool @Args
    if ($LASTEXITCODE -ne 0) { throw "signtool.exe failed with exit code $LASTEXITCODE" }
}

function Invoke-Mage {
    param([string[]]$Args)
    $mage = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\mage.exe"
    if (-not (Test-Path $mage)) { $mage = "mage.exe" }
    & $mage @Args
    if ($LASTEXITCODE -ne 0) { throw "mage.exe failed with exit code $LASTEXITCODE" }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Release Artifact Signing" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Sign all assemblies
Write-Step "Signing assemblies (Authenticode SHA256)..."
$assemblies = Get-ChildItem -Path $BuildOutputDir -Recurse -Include "*.dll", "*.exe"
foreach ($asm in $assemblies) {
    Invoke-Signtool "sign", "/fd", "SHA256", "/a", "/f", "`"$CertFile`"", "/p", $CertPassword, "/tr", $TimestampServer, "/td", "SHA256", "`"$($asm.FullName)`""
    if (-not $Quiet) { Write-Host "  Signed: $($asm.Name)" -ForegroundColor Green }
}
Write-Host "Signed $($assemblies.Count) assemblies" -ForegroundColor Green

# Step 2: Sign VSTO manifest
Write-Step "Signing VSTO application manifest..."
$manifestPath = Join-Path $BuildOutputDir "Som3a_WPF_UI.dll.manifest"
if (Test-Path $manifestPath) {
    Invoke-Mage "-Sign", "`"$manifestPath`"", "-CertFile", "`"$CertFile`"", "-Password", $CertPassword
    Write-Host "Signed: Som3a_WPF_UI.dll.manifest" -ForegroundColor Green
} else {
    Write-Host "WARNING: Application manifest not found — signing skipped" -ForegroundColor Yellow
}

# Step 3: Update VSTO deployment manifest  
Write-Step "Signing VSTO deployment manifest..."
$vstoPath = Join-Path $BuildOutputDir "Som3a_WPF_UI.vsto"
if (Test-Path $vstoPath) {
    Invoke-Mage "-Update", "`"$vstoPath`"", "-AppManifest", "`"$manifestPath`"", "-CertFile", "`"$CertFile`""
    Write-Host "Signed: Som3a_WPF_UI.vsto" -ForegroundColor Green
} else {
    Write-Host "WARNING: VSTO deployment manifest not found — signing skipped" -ForegroundColor Yellow
}

# Step 4: Sign MSI files
Write-Step "Signing MSI installers..."
$msiFiles = Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.msi"
foreach ($msi in $msiFiles) {
    Invoke-Signtool "sign", "/fd", "SHA256", "/a", "/f", "`"$CertFile`"", "/p", $CertPassword, "/tr", $TimestampServer, "/td", "SHA256", "`"$($msi.FullName)`""
    Write-Host "Signed: $($msi.Name)" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "All release artifacts signed successfully" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
