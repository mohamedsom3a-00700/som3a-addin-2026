# Quickstart: Duration Estimator Plugin

## Prerequisites

- Phase 14 (Platform Foundation) complete — Som3a.Domain, Som3a.Contracts, Som3a.Plugin.SDK, Som3a.AI, Som3a.Exporting
- Phase 18 (AI Core Infrastructure) complete — AIOrchestrator, provider adapters, structured parsers
- Phase 20 (BOQ Activity Generator) complete — provides activity input data
- Phase 21 (Relationship Generator) complete — consumes duration output for scheduling
- .NET 8.0 SDK installed
- Existing `WpfApp2` solution with all Phase 14-21 projects

## Project Creation

```powershell
# Add new project to existing solution
dotnet new classlib -n DurationEstimator -o Plugins/DurationEstimator/src
dotnet sln add Plugins/DurationEstimator/src/DurationEstimator.csproj

# Create test project
dotnet new xunit -n DurationEstimator.UnitTests -o Plugins/DurationEstimator/tests/Unit
dotnet sln add Plugins/DurationEstimator/tests/Unit/DurationEstimator.UnitTests.csproj
```

## Package References

```xml
<!-- DurationEstimator.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\..\Som3a.Domain\Som3a.Domain.csproj" />
  <ProjectReference Include="..\..\..\Som3a.Contracts\Som3a.Contracts.csproj" />
  <ProjectReference Include="..\..\..\Som3a.Plugin.SDK\Som3a.Plugin.SDK.csproj" />
  <ProjectReference Include="..\..\..\Som3a.AI\Som3a.AI.csproj" />
  <ProjectReference Include="..\..\..\Som3a.Exporting\Som3a.Exporting.csproj" />
  <ProjectReference Include="..\..\..\Som3a.Infrastructure\Som3a.Infrastructure.csproj" />
</ItemGroup>
```

## Build & Test

```powershell
# Build plugin
dotnet build Plugins/DurationEstimator/src/DurationEstimator.csproj

# Run unit tests
dotnet test Plugins/DurationEstimator/tests/Unit/DurationEstimator.UnitTests.csproj

# Full solution build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## Implementation Order (Recommended)

1. **CalendarEngine** — No dependencies; supports all other calculations
2. **ProductivityEngine** — Core formula; unit-testable in isolation
3. **IBenchmarkLibrary** — Built-in data + custom storage
4. **DurationCalculator** — Combines engine + calendar + benchmarks
5. **DurationEstimatorPlugin** — Plugin class with IPlugin lifecycle
6. **UI Page** — WPF Shell page for user interaction
7. **Export** — Excel export + scheduling pipeline writer
8. **VarianceAnalyzer** — 3-point analysis (after core calc works)
9. **AIProductivitySuggestor** — AI integration (requires Som3a.AI)
10. **AnomalyDetector** — Statistical analysis (after AI integration)

## Key Files

| File | Purpose |
|------|---------|
| `specs/021-duration-estimator-plugin/spec.md` | Feature specification |
| `specs/021-duration-estimator-plugin/plan.md` | Implementation plan |
| `specs/021-duration-estimator-plugin/data-model.md` | Entity definitions |
| `specs/021-duration-estimator-plugin/contracts/` | Interface contracts |
| `specs/021-duration-estimator-plugin/checklists/requirements.md` | Quality checklist |
