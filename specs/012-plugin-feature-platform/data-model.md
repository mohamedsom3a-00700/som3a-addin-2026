# Data Model: Plugin & Feature Platform

## Entity: Module

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique module identifier (e.g., "som3a.project-analysis") |
| Version | string | Semantic version (e.g., "1.0.0") |
| DisplayName | string | Human-readable name for UI |
| Description | string | Short description of module purpose |
| State | ModuleState | Current lifecycle state (see below) |
| Capabilities | CapabilityFlags | Bitmask/flag of capabilities (Pages, Commands, RibbonActions) |
| AssemblyPath | string | Filesystem path to the module assembly |
| Settings | IDictionary | Optional settings key/value pairs declared by module |

### ModuleState Enum

```
Registered → Loading → Active → Failed (terminal)
                                  → Unloaded (terminal, via diagnostics)
                                  → Disabled (terminal, via diagnostics)
```

| State | Description |
|-------|-------------|
| Registered | Module discovered and metadata cached; assembly not loaded |
| Loading | Assembly load in progress; loading spinner visible |
| Active | Module loaded, initialized, and ready for use |
| Failed | Module load or initialization failed; manual retry required |
| Unloaded | Module explicitly unloaded via diagnostics UI; assembly unloaded |
| Disabled | Module explicitly disabled by user; not loaded on navigation |

### State Transition Rules

- Registered → Loading: On first navigation to module's page or first command execution
- Loading → Active: Assembly loaded, initialized successfully
- Loading → Failed: Assembly load or initialization threw exception
- Active → Unloaded: User initiates unload from diagnostics UI
- Active → Disabled: User disables module from diagnostics UI
- Failed → Registered: User clicks "Retry" from diagnostics UI (reverts to Registered then Loading)

---

## Entity: ModuleManifest

| Field | Type | Description |
|-------|------|-------------|
| id | string | Unique module identifier |
| version | string | Semantic version |
| displayName | string | Human-readable name |
| description | string | Short description |
| hash | string | SHA256 hash of assembly file |
| hashAlgorithm | string | Always "SHA256" |
| capabilities | string[] | Array: "pages", "commands", "ribbonActions" |
| dependencies | string[] | Optional: IDs of modules this module depends on |

Stored as `module.json` alongside each module assembly in `Modules/`.

---

## Entity: ModuleRegistry (In-Memory)

| Field | Type | Description |
|-------|------|-------------|
| Modules | Dictionary<string, Module> | All known modules, keyed by Id |
| CapabilitiesIndex | multi-index | Cross-reference: capabilities → list of module IDs |
| DependencyGraph | DirectedGraph | Module dependency relationships for validation |

### Validation Rules

1. **Duplicate ID**: Registration of module with existing Id → rejected with error message
2. **Contract version**: Module contract version must match host's expected version
3. **Missing dependencies**: Required dependency modules must be registered before dependent module
4. **Circular dependencies**: Module A depends on B depends on A → rejected at registration
5. **Assembly integrity**: SHA256 hash mismatch → module rejected
6. **Contract compliance**: Module must implement all required interfaces → rejected with specific errors

---

## Entity: DiagnosticsSnapshot

| Field | Type | Description |
|-------|------|-------------|
| ModuleId | string | Module identifier |
| State | ModuleState | Current lifecycle state |
| Version | string | Version string |
| MemoryBytes | long | Approximate memory usage (GC.GetTotalMemory estimate) |
| LoadTimeMs | long | Milliseconds taken to load and initialize |
| LastError | string | Last error message if in Failed state |
| Capabilities | string[] | Active capabilities list |
