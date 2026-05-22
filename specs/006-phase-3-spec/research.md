# Research: Theme Engine 2.0

**Phase**: 0 — Research & Unknowns Resolution

## Research Topics

### 1. Accent Color Variant Generation

**Decision**: 8 predefined accent color swatches only. No runtime color interpolation for hover/pressed/glow/border/subtle variants. Variants are authored as static hex values per swatch.

**Rationale**: The spec clarifies (Q1) that users select from exactly 8 presets — no custom hex input or color picker. Since accent colors are predefined, each swatch can carry hand-tuned hover/pressed/glow variants baked into the token file, avoiding the complexity and visual unpredictability of runtime color derivation.

**Alternatives considered**: Runtime HSL manipulation to auto-generate variants from any hex input (rejected — adds scope for no user benefit since colors are predefined).

### 2. Safe Mode Detection & Behavior

**Decision**: WindowRenderModeDetector auto-detects VSTO hosting at startup using GPU capability, transparency support, and DPI checks. Safe mode activates silently (Q2) with no user notification.

**Rationale**: Excel VSTO rendering pipelines differ from standalone WPF. Auto-detection prevents black window rendering and transparency artifacts. Silent activation avoids user confusion.

**Detected triggers**: VSTO hosting detected, GPU acceleration unavailable, extreme DPI (>3.0x), transparency instability.

**Safe mode behaviors**: Disables fade animations, uses lower-opacity shadow variants (Shadow.*.Safe), disables backdrop effects, forces AllowsTransparency="False".

### 3. Theme Switching Performance

**Decision**: ThemeManager uses debouncing (≈150ms coalesce window), thread-safe event dispatch via Dispatcher.InvokeAsync(), and safe dictionary fallback in try/catch.

**Rationale**: Rapid theme clicks (10+ in succession) should coalesce to the final selection only. WPF resource dictionary operations must run on UI thread. Failure to load a new theme must preserve the current theme.

**Performance budget**: Theme switch <1s total (SC-001), animations ≤200ms (Constitution §IX).

### 4. Control VSM State Standardization

**Decision**: All 7 control types (Button, TextBox, ComboBox, CheckBox, RadioButton, ToggleButton, DataGrid) use VisualStateManager (VSM) with Normal, Hover, Focused, Pressed, Disabled states.

**Rationale**: VSM provides declarative state management that respects theme resources via DynamicResource. Triggers-based approach was rejected because VSM offers cleaner separation of visual states and easier theme adaptation.

**States per control**: Each control implements 5 states with smooth transitions (≤150ms) using CubicEase(EaseOut) for GPU-friendly easing.

### 5. DPI Scaling Strategy

**Decision**: DynamicResource-based sizing for all scalable elements (theme cards, swatches, popups). SnapsToDevicePixels="True" and UseLayoutRounding="True" on all controls. No imperative DPI scaling code in window code-behind.

**Rationale**: DynamicResource allows theme definition files to define sizing tokens (ThemeCardWidth, AccentSwatchSize) that scale with DPI context. Layout rounding and pixel snapping prevent sub-pixel rendering artifacts without manual scaling logic.

**Supported scales**: 100%, 125%, 150%, 200%.

### 6. Keyboard Accessibility

**Decision**: All interactive controls have FocusVisualStyle using Glow.Focus, AutomationProperties.Name on all interactive elements, logical Tab order, Enter/Space activation for theme cards and swatches.

**Rationale**: Keyboard navigation is the foundation of WCAG 2.1 AA compliance. Focus glow provides visible indicator without relying on system focus rectangles. AutomationProperties enables screen reader support.

### 7. Resource Loading Order

**Decision**: ThemeResources.xaml loads dictionaries in this order: Primitive Tokens → Semantic Tokens → Typography → Spacing → Radius → Elevation → Motion → ZIndex → Opacity → ComponentTokens → Effects (Shadows, Glow, Animations) → Control Styles → Window Styles → Theme Overrides.

**Rationale**: Prescribed by Constitution §XV. Ensures no resource is referenced before it is defined. Theme overrides (Dark/Light/Custom) are swapped at runtime by ThemeManager.
