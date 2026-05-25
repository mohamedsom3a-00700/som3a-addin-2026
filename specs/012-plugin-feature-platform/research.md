# Research: Plugin & Feature Platform

## 1. Module Contract Patterns for .NET

### Decision
Use explicit custom interfaces (IModule, IModuleRegistry, IPluginLoader) instead of MEF or MAF. MEF's implicit discovery is excessive for internal-only modules; MAF's AppDomain isolation is unnecessary for v1 and adds complexity.

### Rationale
- Phase 6 already has ServiceContainer (custom DI) and ModuleRegistry — custom contracts integrate naturally
- No need for MEF's [Import]/[Export] attribute-driven model when all modules are compiled against the same interfaces
- Simpler debugging and type resolution without MEF's catalog/composition container overhead
- Strong contracts enable compile-time validation of module compliance

### Alternatives Considered
- **MEF (System.ComponentModel.Composition)**: Added complexity from attribute-based discovery; conflicts with existing custom DI
- **MAF (System.AddIn)**: AppDomain isolation adds cross-domain serialization overhead; value not justified for internal-only modules
- **Reflection-only convention scanning**: Less type-safe; harder to enforce contract compliance

---

## 2. Lazy Loading for .NET Framework 4.8

### Decision
Use `Assembly.LoadFrom` with a manifest cache. Modules are discovered by scanning `Modules/` directories for assemblies containing known contract types, then lazily loaded via `Assembly.LoadFrom` on first access.

### Rationale
- `Assembly.LoadFrom` allows loading from arbitrary paths outside the app base directory
- Manifest cache (module ID → metadata) built at startup from file scan — no assemblies loaded into memory
- Actual assembly load deferred until first navigation/command invocation
- Works within Excel VSTO hosting without special AppDomain configuration

### Alternatives Considered
- **Assembly.Load(byte[])**: Loads assembly from byte array; prevents proper unload and complicates debugging
- **Assembly.ReflectionOnlyLoadFrom**: Loads for inspection only; cannot execute types
- **Custom AssemblyLoadContext**: .NET Framework 4.8 has limited support; not reliable for VSTO scenarios
- **Pre-load all assemblies at startup**: Defeats the purpose of lazy loading; increases startup time

### Key Considerations for VSTO
- Assemblies loaded into VSTO host process share the default load context
- Module assemblies must be in a well-known location relative to the add-in assembly
- Assembly resolution (AssemblyResolve event) must handle module assembly dependencies

---

## 3. Assembly Integrity Validation

### Decision
Compute SHA256 hash of module assembly bytes at load time and compare against a stored manifest hash. The manifest is a JSON file deployed alongside each module assembly.

### Rationale
- SHA256 is .NET built-in (`System.Security.Cryptography.SHA256`) — no external dependencies
- Hash stored in module manifest (module.json) alongside the assembly
- Validation occurs before `Assembly.LoadFrom` — invalid assemblies are rejected early
- Catches file corruption and accidental tampering without the complexity of strong-name signing

### Alternatives Considered
- **Strong-name signing**: Requires key management infrastructure; overkill for internal-only modules
- **Authenticode signing**: Certificate management overhead; valuable for external distribution but unnecessary for internal team
- **No validation (trust on sight)**: Spec (Q3 clarification) explicitly requires integrity checks

### Implementation Note
```csharp
byte[] assemblyBytes = File.ReadAllBytes(path);
byte[] hash = SHA256.Create().ComputeHash(assemblyBytes);
string computed = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
string expected = manifest.Hash;
if (computed != expected) throw new InvalidOperationException($"Assembly integrity check failed for {moduleId}");
```

---

## 4. Fault Isolation in VSTO

### Decision
In-process fault containment using try/catch wrappers around all module lifecycle calls (load, initialize, navigate, command execute). No AppDomain isolation for v1.

### Rationale
- Excel VSTO hosting makes AppDomain creation unreliable — VSTO add-ins run in the Excel process
- Cross-AppDomain calls require MarshalByRefObject and serialization, adding complexity
- Try/catch at every module boundary provides sufficient isolation for internal modules
- Permanent Failed state (per clarified spec) prevents retry-loops on unstable modules
- Diagnostics panel allows user to manually retry failed modules

### Alternatives Considered
- **Separate AppDomain per module**: Excel VSTO add-ins cannot safely create/manage isolated AppDomains; cross-domain calls add significant complexity
- **Out-of-process modules via IPC**: Massive overkill for internal-only modules; network/socket management overhead
- **No isolation (fail-fast)**: Violates spec requirement (FR-005, US3)

### Fault Boundaries
| Boundary | Action |
|----------|--------|
| Module discovery/scan | Skip corrupt assembly, log error |
| Module initialization | Catch exception → set state to Failed, log, continue |
| Module page navigation | Catch exception → show inline error within content area |
| Module command execution | Catch exception → log, show toast notification |
| Module ribbon action click | Catch exception → log, show toast notification |

---

## 5. Ribbon Registration Patterns

### Decision
Modules register ribbon actions via a callback pattern during initialization. The module's `Initialize` method receives an `IRibbonRegistrar` interface that the module calls to register buttons, menus, and their click handlers.

### Rationale
- Existing VSTO ribbon infrastructure uses callbacks (GetCustomUI, onAction callbacks)
- `IRibbonRegistrar` abstracts the VSTO ribbon implementation from modules
- Modules don't need direct references to VSTO types (Microsoft.Office.Tools, etc.)
- Registration callbacks can be cached and replayed at ribbon reload time

### Alternatives Considered
- **Ribbon XML-based registration (module provides its own ribbon XML)**: More flexible but requires parsing and merging multiple ribbon XML fragments at runtime; risk of ID conflicts
- **Attribute-based ribbon discovery**: Less explicit; harder to debug registration issues
- **Direct VSTO ribbon manipulation from modules**: Creates tight coupling to VSTO infrastructure; violates MVVM

### Implementation Note
```csharp
public interface IRibbonRegistrar
{
    void AddButton(string id, string label, string tooltip, Action onClick);
    void AddMenu(string id, string label, IReadOnlyList<RibbonMenuItem> items);
    void AddToggleButton(string id, string label, Action<bool> onToggle, bool isChecked);
}
```
