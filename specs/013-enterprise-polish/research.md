# Research: Enterprise Polish

**Branch**: `013-enterprise-polish` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

## 1. WPF Performance Optimization Patterns

### Freezable Objects
- WPF freezable objects (SolidColorBrush, LinearGradientBrush, Pen, Transform, GeometryDrawing) consume less memory and improve rendering when frozen.
- Freeze after creation and before adding to ResourceDictionary.
- Pattern: `brush.Freeze();` or `x:Shared="False"` for instances that must remain unfrozen.
- Impact: ~30% memory reduction for brush-heavy UIs per Microsoft documentation.

### UI Virtualization
- WPF VirtualizingStackPanel is the default for ListBox, ListView, DataGrid.
- `VirtualizationMode="Recycling"` reuses containers instead of creating new ones — critical for DataGrid with 1000+ rows.
- `ScrollUnit="Item"` for pixel-based scrolling vs `"Pixel"` for item-based.
- Known issue: Virtualization breaks when DataGrid is inside ScrollViewer — ensure no nested ScrollViewer in DataGrid templates.

### Animation Budget
- WPF animations run on the composition thread when using independent animations (animations of RenderTransform, Opacity, etc.).
- Dependent animations (animations of layout-affecting properties like Width, Height, Margin) run on the UI thread and cause layout passes.
- Preference: Use `RenderTransform` for positioning animations, `Opacity` for fade, `ScaleTransform` for size — all independent.
- All storyboards in existing codebase must be audited to confirm independent animation targets.

### Startup Optimization
- `SplashScreen` class for early visual feedback.
- Defer non-critical service initialization using `Dispatcher.BeginInvoke(DispatcherPriority.Background, ...)`.
- Avoid synchronous XAML parsing of large resource dictionaries — split into smaller dictionaries loaded on demand.

## 2. WPF Accessibility Patterns

### AutomationProperties
- `AutomationProperties.Name` — primary label for screen readers; should match visible text.
- `AutomationProperties.HelpText` — additional description for complex controls.
- `AutomationProperties.LabeledBy` — reference to a `Label` or `TextBlock` that serves as the label.
- `AutomationProperties.AccessibilityView` — set to `Raw`, `Control`, or `Content` to control what automation peers see.

### Keyboard Navigation
- `IsTabStop="False"` on non-interactive elements (labels, separators).
- `KeyboardNavigation.TabNavigation="Once"` for tab-scope within a container.
- `KeyboardNavigation.DirectionalNavigation="Cycle"` for arrow-key navigation within container.
- `AccessKey` modifier + underscore in header text for legacy access key support.

### WCAG 2.1 AA Contrast
- Ratio = (L1 + 0.05) / (L2 + 0.05) where L1 is relative luminance of lighter color.
- Minimum 4.5:1 for normal text (<18pt or <14pt bold).
- Minimum 3:1 for large text (>=18pt or >=14pt bold).
- WPF Color helper: `Color.GetBrightness()` is inaccurate — use formula:
  `L = 0.2126 * R + 0.7152 * G + 0.0722 * B` where R, G, B are sRGB linearized.
- Windows Accessibility Insights can measure and report contrast ratios.

## 3. WPF DPI & Multi-Monitor Patterns

### Per-Monitor DPI in WPF
- .NET Framework 4.6.2+ (our 4.8 target) supports per-monitor DPI.
- Requires app manifest with `<dpiAwareness>PerMonitorV2</dpiAwareness>`.
- WPF `Visual` has `DpiChanged` event — handle to resize/reposition.
- `PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice` for current DPI scale.

### DPI-Aware Popup Positioning
- ComboBox popup `PlacementTarget` binding must be to the ComboBox element, not a parent.
- `Popup.Placement="Bottom"` with `Popup.PlacementRectangle` offset based on current DPI scale.
- `Popup.AllowsTransparency="False"` is already set — required for Excel VSTO.
- ContextMenu placement is controlled by `PlacementTarget` and `Placement` properties.

### Multi-Monitor Mixed DPI
- Moving windows between monitors at different DPIs triggers `DpiChanged` on the window.
- WPF re-renders the window at the new DPI automatically — but layout may not re-flow.
- `Width` and `Height` in device-independent pixels (DIPs) scale automatically.
- Issues occur when XAML has hardcoded pixel values that don't scale.

## 4. Excel VSTO Rendering Observations

### Prior Findings (from Phase 3)
- Black window artifacts caused by `AllowsTransparency="True"` — resolved by setting to `False`.
- DPI rendering issues in ComboBox popup — resolution in progress.
- Safe mode (software rendering) needed for some VDI/citrix scenarios.
- Theme switching inside Excel host works but needs performance validation.

### Common Excel VSTO Issues
- WindowChrome can conflict with Excel's own window message handling.
- No BlurEffect on scrollable areas — documented rule.
- Popup clipping at window boundaries — `PlacementTarget` binding is key.
- Resource Dictionary loading order must be deterministic — Excel host can have timing-sensitive initialization.

## 5. Memory Profiling Approach

### Tools Available
- **Visual Studio Diagnostic Tools** — memory usage, CPU usage, timeline.
- **dotMemory** — heap analysis, retention paths.
- **PerfView** — ETW-based profiling for .NET Framework.
- **Windows Performance Recorder (WPR)** — system-wide tracing.

### Key Metrics
- Private working set (baseline).
- GC heap size (Gen0, Gen1, Gen2, Large Object Heap).
- Number of WPF visuals/UIElement instances.
- Resource dictionary loading count (avoid duplicate loads).
- Event handler subscription count (detect leaks).

## 6. ValidationEngine Extension Design

### Current State
- `ValidationEngine.cs` exists with `ScanForMissingTokens()`, `ScanForInlineColors()`, `ScanForDuplicateStyles()`, `ScanForInvalidResources()`.
- TODO at line ~185: "TODO: implement proper XAML/dictionary resolver instead of simple type check".

### Extension Design
- Use `Application.Current.Resources.MergedDictionaries` to enumerate all loaded dictionaries.
- For unloaded dictionaries, use `XamlReader.Load()` with `ParserContext` to scan without loading into visual tree.
- Pattern: Parse XAML as XML, extract `Color="#..."` attributes, report location.
- Pattern: Parse XAML as XML, extract `CornerRadius`, `Margin`, `Padding` literal values not using DynamicResource.
- Pattern: Cross-reference all `x:Key` values in `Theme/Base/` and `Theme/Controls/` to find missing token references.
