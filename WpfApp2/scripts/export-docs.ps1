<#
.SYNOPSIS
    Export all documentation guides with SemVer version matching the release.
.DESCRIPTION
    Exports 6 documentation guides (User, Admin, Plugin SDK, Architecture, AI Setup, Troubleshooting)
    with version headers and creates a documentation manifest.
.PARAMETER Version
    SemVer version for the release. Default: auto-detect.
.PARAMETER OutputDir
    Output directory for exported docs. Default: build/docs/
.PARAMETER Quiet
    Suppress verbose output.
#>

param(
    [string]$Version = "",
    [string]$OutputDir = "",
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")

if (-not $Version) {
    $asmPath = Join-Path $repoRoot "Properties\AssemblyInfo.cs"
    if (Test-Path $asmPath) {
        $match = [regex]::Match((Get-Content $asmPath -Raw), 'AssemblyVersion\("(\d+\.\d+\.\d+)"\)')
        if ($match.Success) { $Version = $match.Groups[1].Value }
    }
    if (-not $Version) { $Version = "1.0.0" }
}

if (-not $OutputDir) { $OutputDir = Join-Path $repoRoot "build\docs" }

$guides = @(
    @{ Name = "User Guide"; FileName = "UserGuide.md"; Source = "UserGuide.md"; Title = "User Guide" }
    @{ Name = "Admin Guide"; FileName = "AdminGuide.md"; Source = "AdminGuide.md"; Title = "Admin Guide" }
    @{ Name = "Plugin SDK Guide"; FileName = "PluginSDKGuide.md"; Source = "PluginSDKGuide.md"; Title = "Plugin SDK Guide" }
    @{ Name = "Architecture Guide"; FileName = "ArchitectureGuide.md"; Source = "ArchitectureGuide.md"; Title = "Architecture Guide" }
    @{ Name = "AI Provider Setup Guide"; FileName = "AIProviderSetupGuide.md"; Source = "AIProviderSetupGuide.md"; Title = "AI Provider Setup Guide" }
    @{ Name = "Troubleshooting Guide"; FileName = "TroubleshootingGuide.md"; Source = "TroubleshootingGuide.md"; Title = "Troubleshooting Guide" }
)

$exportedGuides = @()

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Documentation Export (v$Version)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

foreach ($guide in $guides) {
    $sourcePath = Join-Path $repoRoot "Docs" $guide.Source
    $targetPath = Join-Path $OutputDir "$($guide.FileName)"

    if (Test-Path $sourcePath) {
        $content = Get-Content $sourcePath -Raw
        $versionHeader = "> **Version**: $Version  " + [Environment]::NewLine + "> **Exported**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')  " + [Environment]::NewLine + "> **Guide**: $($guide.Title)" + [Environment]::NewLine + [Environment]::NewLine
        $versionedContent = $versionHeader + $content
        $versionedContent | Set-Content $targetPath
        Write-Host "  [PASS] $($guide.Name) -> $targetPath" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] $($guide.Name): source not found at $sourcePath — creating placeholder" -ForegroundColor Yellow
        $placeholder = @"
> **Version**: $Version
> **Exported**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
> **Guide**: $($guide.Title)

# $($guide.Title) (v$Version)

*This guide will be available in a future update.*

## Overview

This document provides guidance for the $($guide.Name.ToLower()).
"@
        $placeholder | Set-Content $targetPath
    }

    $exportedGuides += @{
        title = $guide.Title
        path = $targetPath
        version = $Version
        exportedAt = (Get-Date -Format "o")
    }
}

# Write documentation manifest
$manifest = @{
    version = $Version
    exportFormat = "Markdown"
    exportedAt = (Get-Date -Format "o")
    guides = $exportedGuides
}
$manifestPath = Join-Path $OutputDir "..\docs-manifest.json"
$manifest | ConvertTo-Json -Depth 3 | Set-Content (Resolve-Path (Join-Path $OutputDir ".."))\docs-manifest.json 2>$null
if (-not (Test-Path $manifestPath)) {
    $manifest | ConvertTo-Json -Depth 3 | Set-Content (Join-Path (Split-Path $OutputDir) "docs-manifest.json")
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Documentation export complete (v$Version)" -ForegroundColor Cyan
Write-Host "Guides exported: $($exportedGuides.Count)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
