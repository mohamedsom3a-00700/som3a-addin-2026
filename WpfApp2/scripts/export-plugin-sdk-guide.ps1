param([string]$Version = "", [string]$OutputDir = "", [switch]$Quiet)
& (Join-Path $PSScriptRoot "export-docs.ps1") -Version $Version -OutputDir $OutputDir -Quiet:$Quiet
