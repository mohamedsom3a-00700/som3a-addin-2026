# Research: CommunityToolkit.Mvvm ViewModel Refactor

**Date**: 2026-05-31
**Feature**: CommunityToolkit.Mvvm ViewModel Refactor

## Decisions

### Decision 1: Use CommunityToolkit.Mvvm Source Generators

**Rationale**: The project already has CommunityToolkit.Mvvm installed (from prior phase work). The `[ObservableProperty]` and `[RelayCommand]` source generators eliminate all manual `INotifyPropertyChanged` boilerplate and custom command classes. This reduces bug surface, improves compile-time safety, and aligns with modern .NET MVVM best practices.

**Alternatives considered**:
- **Continue manual INPC**: Rejected — high maintenance overhead, error-prone.
- **Use ReactiveUI**: Rejected — introduces a second MVVM framework; CommunityToolkit.Mvvm is already in use and is the Microsoft-recommended path.
- **Use PropertyChanged.Fody**: Rejected — weaving-based solution is less transparent than source generators and harder to debug.

### Decision 2: Incremental Batch Migration (5-7 ViewModels per batch)

**Rationale**: Migrating all 20+ ViewModels in a single changeset carries high risk of build breakage, merge conflicts, and difficult debugging. Batches allow continuous validation and immediate rollback if a batch introduces issues.

**Batch order proposed**:
1. Batch 1: `ViewModelBase`, `HomeViewModel`, `ToastViewModel` (low risk, core infrastructure)
2. Batch 2: Dashboard widget ViewModels (isolated, testable independently)
3. Batch 3: Settings-related ViewModels (`SettingsViewModel`, `LanguagePageViewModel`, `DiagnosticsViewModel`)
4. Batch 4: Planning page ViewModels (`BOQActivityGeneratorViewModel`, `DurationEstimatorPageViewModel`, `WBSEditorViewModel`)
5. Batch 5: Remaining ViewModels (`WBSGeneratorViewModel`, `RelationshipGeneratorViewModel`, Primavera ViewModels, `ShellViewModel`, `CommandPaletteViewModel`, etc.)
6. Final: Delete `RelayCommand.cs` and `AsyncRelayCommand.cs`, run full verification.

**Alternatives considered**:
- **Single big-bang**: Rejected — too risky for a mechanical refactor.
- **Migrate by functional area**: Partially overlaps with batch approach; batch count was chosen for practical build validation cadence.

### Decision 3: Refactor Custom Setter Side Effects into Partial Method Hooks

**Rationale**: Some existing ViewModel properties may contain custom logic inside setters (e.g., calling validation, triggering dependent property updates). The CommunityToolkit.Mvvm source generator supports `partial void OnPropertyChanged(string propertyName)` and `partial void OnPropertyChanging(string propertyName)` hooks. Moving side effects into these hooks preserves behavior while allowing the property itself to be source-generated.

**Pattern**:
```csharp
// Before (manual)
private string _name;
public string Name
{
    get => _name;
    set
    {
        _name = value;
        OnPropertyChanged(nameof(Name));
        ValidateName(); // side effect
    }
}

// After (source-generated)
[ObservableProperty]
private string _name;

partial void OnNameChanged(string value)
{
    ValidateName(); // side effect moved to hook
}
```

**Alternatives considered**:
- **Leave manual properties as-is**: Rejected — undermines the goal of eliminating boilerplate.
- **Use `[NotifyPropertyChangedFor]` / `[NotifyCanExecuteChangedFor]`**: Accepted as a supplementary technique when the side effect is simply notifying another property; partial methods are the general fallback.

### Decision 4: Automated Verification Script + CI Gate

**Rationale**: Manual code review alone is insufficient to guarantee that all manual patterns have been removed across 20+ files. An automated script provides deterministic, repeatable verification. Integrating it into the CI/build pipeline prevents regression.

**Script behavior**:
- Scan all `.cs` files in `ViewModels/` for forbidden patterns:
  - Manual `INotifyPropertyChanged` implementation (classes implementing the interface directly instead of inheriting `ObservableObject`)
  - Manual `OnPropertyChanged(string)` calls in setters
  - Usage of `RelayCommand` or `AsyncRelayCommand` from `Models/` or `Helpers/`
  - Properties with explicit backing fields and manual `set { _field = value; OnPropertyChanged(...); }` bodies
- Return non-zero exit code if any forbidden patterns are found.
- CI gate: run script as a pre-build or PR validation step.

**Alternatives considered**:
- **Manual review only**: Rejected — error-prone at scale.
- **One-time script run**: Rejected — CI gate provides ongoing enforcement.

## References

- [CommunityToolkit.Mvvm Source Generators Documentation](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/generators/overview)
- [ObservableProperty Attribute](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/generators/observableproperty)
- [RelayCommand Attribute](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/generators/relaycommand)
