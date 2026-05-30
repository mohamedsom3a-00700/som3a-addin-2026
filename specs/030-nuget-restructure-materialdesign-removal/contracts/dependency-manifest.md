# Contracts: NuGet Restructure & MaterialDesign Removal

**Date**: 2026-05-30

## Overview

No new external interfaces, APIs, or UI contracts are introduced in this phase.

This phase is strictly an internal dependency cleanup. The only "contract" affected is the NuGet package dependency manifest, which is implicitly managed through the `.csproj` files.

## Dependency Manifest Contract (Implicit)

### Before (Phase 1B)

- `MaterialDesignThemes` (WPF host + VSTO add-in)
- `MaterialDesignColors` (WPF host + VSTO add-in)

### After (Phase 1C)

- `FluentIcons.WPF` (WPF host only)
- `Wpf.Ui` (WPF host only)
- `MaterialDesignThemes` — REMOVED
- `MaterialDesignColors` — REMOVED

## Verification

- `dotnet list WpfApp2/Som3a_WPF_UI.csproj package` shows zero MaterialDesign packages.
- `dotnet list Som3aAddin/Som3aAddin.csproj package` shows zero MaterialDesign packages.
