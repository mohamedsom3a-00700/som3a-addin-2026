# Data Model: Release Candidate & Production Packaging

## Entities

### ReleaseCandidate

| Field | Type | Description |
|-------|------|-------------|
| version | SemVer (Major.Minor.Patch) | Release version identifier |
| buildNumber | string | Auto-incrementing build identifier |
| buildDate | DateTime | Timestamp of build completion |
| branch | string | Source branch (feature/phase-26-release-candidate) |
| stages | PipelineStage[] | Ordered list of pipeline stages and their status |
| validationReport | ValidationReport | Aggregate validation results |
| artifact | ReleaseArtifact | Path to built MSI/exe artifact |
| status | CandidateStatus | draft / in-progress / passed / failed / released |

**Status transitions**: draft → in-progress → passed → released; in-progress → failed → in-progress (resume after fix)

### PipelineStage

| Field | Type | Description |
|-------|------|-------------|
| name | string | Stage name: Validation, Optimization, Packaging, Installer, QA |
| status | StageStatus | pending / running / passed / failed / skipped |
| startedAt | DateTime? | When stage began |
| completedAt | DateTime? | When stage ended |
| diagnostics | string[] | Error/diagnostic messages produced by stage |
| canResume | bool | Whether pipeline can resume from this stage after failure |

### ValidationReport

| Field | Type | Description |
|-------|------|-------------|
| uiValidation | ValidationResult | Theme switching, shell navigation, RTL, accessibility checks |
| pluginValidation | ValidationResult | Plugin loading, isolation, sustained use (4hr, zero crashes, <5% memory growth) |
| aiValidation | ValidationResult | Provider prompt outputs, retry handling, structured JSON parsing |
| excelValidation | ValidationResult | Export speed, large workbook support, interop cleanup |
| overall | PassFail | Pass only when all 4 areas pass |

### ValidationResult

| Field | Type | Description |
|-------|------|-------------|
| passed | bool | Whether validation passed |
| details | string[] | List of individual check results |
| duration | TimeSpan | Time taken for validation |
| errors | string[] | Failure details if not passed |

### ReleaseArtifact

| Field | Type | Description |
|-------|------|-------------|
| msiPath | string | Path to MSI installer |
| bundlePath | string? | Path to Burn bootstrapper exe (if applicable) |
| signatureVerified | bool | Whether code signing signature was verified |
| version | string | Version embedded in artifact metadata |

### MSIInstaller

| Field | Type | Description |
|-------|------|-------------|
| productCode | Guid | MSI ProductCode (unique per version) |
| upgradeCode | Guid | Stable UpgradeCode across all versions |
| version | SemVer | Product version |
| includesDotNetRuntime | bool | Whether .NET 8 runtime is bundled |
| requiresAdmin | bool | Whether admin elevation required |
| components | InstallComponent[] | Components installed (add-in, service, shortcuts) |

### InstallComponent

| Field | Type | Description |
|-------|------|-------------|
| type | ComponentType | AddIn / Service / Shortcut / Registry / Config |
| targetPath | string | Installation destination path |
| description | string | Component purpose |

### DocumentationSet

| Field | Type | Description |
|-------|------|-------------|
| guides | DocGuide[] | All exported guides |
| version | SemVer | Matching release version |
| exportFormat | string | Format (PDF, HTML, Markdown) |
| exportedAt | DateTime | Export timestamp |

### DocGuide

| Field | Type | Description |
|-------|------|-------------|
| title | string | Guide title (User Guide, Admin Guide, etc.) |
| path | string | Output file path |
| versioned | bool | Whether version info embedded |

### ProductionConfiguration

| Field | Type | Description |
|-------|------|-------------|
| loggingLevel | string | Error (default) |
| maxLogSizeMB | int | 100 |
| aiProviderEndpoints | AiEndpoint[] | Production AI provider URLs |
| featureFlags | FeatureFlag[] | Production feature toggles |

### DiagnosticsSnapshot

| Field | Type | Description |
|-------|------|-------------|
| capturedAt | DateTime | When snapshot was taken |
| memoryMB | long | Current process memory |
| activePlugins | string[] | Loaded plugin IDs |
| recentOperations | string[] | Last N operations before crash |
| errorContext | string | Exception details and stack trace |
| theme | string | Active theme mode |
| excelVersion | string | Detected Excel version |

## Relationships

- **ReleaseCandidate** has one **ValidationReport** (1:1)
- **ReleaseCandidate** has one **ReleaseArtifact** (1:1)
- **ReleaseArtifact** produces one **MSIInstaller** (1:1)
- **ReleaseCandidate** is associated with one **DocumentationSet** (1:1)
- **ReleaseCandidate** has one **ProductionConfiguration** (1:1)
- **ReleaseCandidate** can produce zero or more **DiagnosticsSnapshot** on crash (1:N)
- **PipelineStage** belongs to one **ReleaseCandidate** (N:1)
