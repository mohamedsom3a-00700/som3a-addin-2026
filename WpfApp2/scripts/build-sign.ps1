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

function Invoke-Native {
    param([string]$FilePath, [string[]]$ArgumentList)
    $proc = Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -ne 0) { throw "$(Split-Path $FilePath -Leaf) exited with code $($proc.ExitCode)" }
}

# Resolve tool paths
$signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
if (-not (Test-Path $signtool)) {
    $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe"
}
if (-not (Test-Path $signtool)) {
    $signtool = "signtool.exe"
}

$mage = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\mage.exe"
if (-not (Test-Path $mage)) {
    $mage = "mage.exe"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Code Signing Script" -ForegroundColor Cyan
Write-Host "Tools: $(Split-Path $signtool -Leaf), $(Split-Path $mage -Leaf)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Sign all DLLs
Write-Step "Signing assemblies with Authenticode (SHA256)..."
$dllFiles = Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.dll"
$count = 0
foreach ($dll in $dllFiles) {
    Invoke-Native -FilePath $signtool -ArgumentList @(
        "sign", "/fd", "SHA256", "/a",
        "/f", $CertFile,
        "/p", $CertPassword,
        "/tr", $TimestampServer,
        "/td", "SHA256",
        "`"$($dll.FullName)`""
    )
    $count++
    if (-not $Quiet) { Write-Host "  Signed: $($dll.Name)" -ForegroundColor Green }
}
Write-Host "Signed $count assemblies" -ForegroundColor Green

# Step 2: Sign VSTO manifest
Write-Step "Signing VSTO manifest..."
$manifestPath = Join-Path $BuildOutputDir "Som3a_WPF_UI.dll.manifest"
if (Test-Path $manifestPath) {
    Invoke-Native -FilePath $mage -ArgumentList @(
        "-Sign", "`"$manifestPath`"",
        "-CertFile", $CertFile,
        "-Password", $CertPassword
    )
    Write-Host "Signed: $manifestPath" -ForegroundColor Green

    # Step 3: Update VSTO deployment manifest
    Write-Step "Updating VSTO deployment manifest..."
    $vstoPath = Join-Path $BuildOutputDir "Som3a_WPF_UI.vsto"
    if (Test-Path $vstoPath) {
        Invoke-Native -FilePath $mage -ArgumentList @(
            "-Update", "`"$vstoPath`"",
            "-AppManifest", "`"$manifestPath`"",
            "-CertFile", $CertFile
        )
        Write-Host "Updated: $vstoPath" -ForegroundColor Green
    }
} else {
    Write-Host "WARNING: $manifestPath not found — manifest signing skipped" -ForegroundColor Yellow
}

# Step 4: Sign MSI if present
$msiFiles = Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.msi"
foreach ($msi in $msiFiles) {
    Write-Step "Signing MSI: $($msi.Name)..."
    Invoke-Native -FilePath $signtool -ArgumentList @(
        "sign", "/fd", "SHA256", "/a",
        "/f", $CertFile,
        "/p", $CertPassword,
        "/tr", $TimestampServer,
        "/td", "SHA256",
        "`"$($msi.FullName)`""
    )
    Write-Host "Signed: $($msi.Name)" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Code signing complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
