# Som3a Addin 2026

A professional Excel VSTO Add-in built with C#, .NET Framework 4.8, and WPF.

## Overview

This add-in extends Microsoft Excel with custom ribbon UI elements and integrated WPF dialogs for enhanced user productivity. The project includes database connectivity to Oracle and SQL Server, with modern MVVM architecture using CommunityToolkit.Mvvm.

## Features

- Custom Excel Ribbon with user-friendly interface
- WPF-based dialog windows integrated into Excel
- Oracle database connectivity
- SQL Server database connectivity
- WebView2 integration for modern web content
- MVVM architecture with CommunityToolkit.Mvvm

## Technologies

- **Framework**: .NET Framework 4.8
- **Language**: C# 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Office Integration**: VSTO (Visual Studio Tools for Office)
- **Database**: Oracle Managed Data Access, System.Data.SqlClient
- **MVVM**: CommunityToolkit.Mvvm 8.4.2
- **WebView**: Microsoft.Web.WebView2

## Architecture

```
Som3a Addin 2026/
├── Som3a Addin 2026/      # Main VSTO Excel Add-in project
│   ├── Ui/                # WPF UI components
│   ├── Properties/        # Assembly and project properties
│   ├── Resources/         # Icons and images
│   └── Ribbon1.cs         # Custom ribbon implementation
├── Som3a.Shared/          # Shared code and utilities
├── WpfApp2/               # WPF UI library
└── Docs/                  # Documentation
```

## Installation

### Prerequisites

- Visual Studio 2022 or later
- Microsoft Office 2016 or later (Excel)
- .NET Framework 4.8
- Oracle Database (optional, for database features)

### Build and Run

1. Open `Som3a Addin 2026.slnx` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Debug or Release configuration)
4. Press F5 to run, or install the add-in to Excel

### Deploying

The add-in can be deployed using:
- Visual Studio Publish wizard
- ClickOnce deployment
- MSI installer

## Usage

After installation, the add-in appears in the Excel ribbon with custom buttons for:
- Data operations
- Report generation
- Links management
- Settings and configuration

## Configuration

- Edit `app.config` for connection strings
- Modify `Properties/Settings.settings` for application settings

## License

Private repository - All rights reserved.