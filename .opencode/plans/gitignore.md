# .gitignore for Som3a Add-in 2026

**File**: `.gitignore`
**Status**: Saved for creation

---

## Implementation

Create `.gitignore` file in project root with the following content:

```gitignore
# =============================================================================
# Som3a Add-in 2026 - .gitignore
# Best practices for .NET Framework 4.8 + WPF + VSTO project
# =============================================================================

# -----------------------------------------------------------------------------
# Visual Studio / IDE
# -----------------------------------------------------------------------------
.vs/
.vscode/
[Ll]og/
[Oo]bj/
[Bb]in/
*.suo
*.user
*.userosscache
*.sln.docstates
*.sln.*.user
*.rsuser
*.userprefs
*~

# -----------------------------------------------------------------------------
# Build Results
# -----------------------------------------------------------------------------
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
[Ll]og/
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og/
*.Cache
[Mm]inicab[Nn]uget/
Ankh.NoLoad

# -----------------------------------------------------------------------------
# .NET
# -----------------------------------------------------------------------------
*.snk
*.dll
*.exe
*.pdb
*.mdf
*.ldf
*.opt
*.aps
*.pch
*.jl
*.tlb
*.tlh
*.bak
*.log
*.ilk
*.lib
*.exp
*.vspscc
*.vssscc

# NuGet
*.nupkg
packages/
!**/packages/repositories.config
!**/packages/build/

# -----------------------------------------------------------------------------
# VSTO (Visual Studio Tools for Office)
# -----------------------------------------------------------------------------
/bin/
/obj/
Debug/
Release/
*.manifest
ClickOnce/
Publish/
*.application
*.app
*.app.manifest
*.publish*
*.prf
*.pubxml*
*.publi?

# -----------------------------------------------------------------------------
# Excel Add-in specific
# -----------------------------------------------------------------------------
*.vsto
*.xll
*.xlam
*.xlsx~
*.xlsm~
~$*.xlsx
~$*.xlsm
~$*.pptx
~$*.pptm
*.tiff
*.oft

# -----------------------------------------------------------------------------
# User Settings
# -----------------------------------------------------------------------------
Settings/
Properties/PublishProfiles/
Properties/ServiceReferences/
ClientBin/
[Ss]tyleCop.[Pp]arts/
[Ss]tyle[Cc]op.
_ReSharper.*
_ReSharper*/
*.DotSettings.user
*.DotSettings

# -----------------------------------------------------------------------------
# Third-party
# -----------------------------------------------------------------------------
/packages/
/lib/
/lib64/
/third-party/
*.jar
*.war
*.ear
*.zip
*.tar.gz
*.rar

# -----------------------------------------------------------------------------
# Documentation & Misc
# -----------------------------------------------------------------------------
*.md.html
*.chm
*.pdf
*.tmp
*.temp
.DS_Store
Thumbs.db
*.swp
*.swo
*~
.idea/

# -----------------------------------------------------------------------------
# SQL & Database
# -----------------------------------------------------------------------------
*.mdf
*.ldf
*.ndf
*.sdf
*.bak

# -----------------------------------------------------------------------------
# Certificates & Secrets (NEVER commit)
# -----------------------------------------------------------------------------
*.pfx
*.cer
*.p12
*.pem
*.key
*.pub
*.secret
*.connectionStrings.config
appsettings.*.json
secrets.config
```

---

## Coverage

| Category | Included |
|----------|----------|
| Visual Studio | ✅ `.vs/`, `.suo`, `.user` |
| Build outputs | ✅ `bin/`, `obj/`, Debug/Release |
| .NET artifacts | ✅ `.dll`, `.exe`, `.pdb`, `.snk` |
| VSTO/Office | ✅ `.vsto`, `.xll`, `.xlam`, temp files |
| NuGet | ✅ `packages/`, `.nupkg` |
| IDE settings | ✅ `.vscode/`, `.idea/` |
| Secrets | ✅ `.pfx`, `.pem`, connection strings |
| OS artifacts | ✅ `Thumbs.db`, `.DS_Store` |