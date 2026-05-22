# Research: Design System Core

**Branch**: `004-design-system-core` | **Date**: 2026-05-22

## Research Task 1: WPF DynamicResource Limitations for Non-DP Properties

**Decision**: Use ComponentTokens.xaml with hardcoded `sys:Double` / `sys:Int32` values alongside DynamicResource where possible.

**Rationale**: WPF `DynamicResource` only works on dependency properties. For `CornerRadius`, `Thickness`, and `sys:Double` values used in attribute syntax (e.g., `CornerRadius="{DynamicResource MediumRadius}"`), DynamicResource works because these are DP-backed. For `FontWeight`, `FontFamily`, and similar properties on styles, DynamicResource works when set via `Setter`. The limitation only applies to `Color` values inside `GradientStop.Color` and `ColorAnimation.To`, which require code-behind or `SolidColorBrush` workarounds.

**Alternatives considered**:
- StaticResource everywhere: Rejected per Constitution III (DynamicResource-Only mandate).
- Inline values with code-behind assignment: Rejected per Constitution III.
- GradientStop replacements via SolidColorBrush: Accepted for `GradientStop` scenarios (V1 violations in SettingsWindow.xaml).

## Research Task 2: GradientStop Inline Color Replacement Strategy

**Decision**: Replace inline `GradientStop Color="#"` with `DynamicResource` referencing `Color.*` semantic tokens. WPF supports `DynamicResource` on `GradientStop.Color` since .NET 3.0+.

**Rationale**: The 6 inline GradientStop colors in SettingsWindow.xaml (lines 206-258) should use existing or new `Color.*` tokens. For Dark theme cards: `Color.Background.RootStart` and `Color.Background.RootEnd` already exist. For Light theme cards: need `Color.Background.RootStart.Light` and `Color.Background.RootEnd.Light` tokens in LightColors.xaml overrides. For Custom theme: same as Dark by default.

**Alternatives considered**:
- SolidColorBrush replacement: Rejected — theme cards intentionally use gradients for visual appeal.
- Code-behind color assignment: Rejected — violates DynamicResource mandate.

## Research Task 3: ColorAnimation Inline Hex Replacement

**Decision**: Create `Color.*` semantic tokens for animation target values and reference them via `DynamicResource`.

**Rationale**: WPF `ColorAnimation.To` does NOT support `DynamicResource` directly — it requires a frozen `Color` value. The workaround is to use `Storyboard` with `ObjectAnimationUsingKeyFrames` and `DiscreteObjectKeyFrame` referencing a `DynamicResource`, or to replace the ColorAnimation with an Opacity animation that uses `Opacity.*` tokens. For the 2 violations (WindowAnimations.xaml:82 `#22FF4757` and ScrollViewerStyles.xaml:78 `#66FFFFFF`):
- `#22FF4757`: Close button hover danger-red → replace with `Opacity.Pressed` on existing danger brush, or create `Color.Animation.ButtonHoverDanger` token.
- `#66FFFFFF`: ScrollViewer thumb fade → replace with `Opacity.Ghost` (value 0.4 ≈ `#66FFFFFF` at 0.4 alpha on white), or create `Color.Animation.ThumbFade` token.

**Alternatives considered**:
- Keep inline hex in animations only: Rejected — spec requires zero inline hex outside Base/ token files.
- Replace ColorAnimation with OpacityAnimation: Accepted where semantically equivalent (thumb fade, button hover opacity).

## Research Task 4: Elevation.xaml Placement (V8 Violation)

**Decision**: Move DropShadowEffect definitions from `Base/Elevation.xaml` to `Effects/Shadows.xaml`, and convert `Base/Elevation.xaml` to an abstraction-only dictionary with `sys:Double` offset keys (BlurRadius, ShadowDepth, Opacity as separate Double tokens). Controls reference `Elevation.*` keys for abstraction; `Shadow.*` keys provide the actual DropShadowEffect instances.

**Rationale**: Constitution XII mandates "All effects MUST originate from centralized dictionaries" and "Inline effects are prohibited." Having DropShadowEffect in `Base/` violates this principle. By making Elevation.xaml an abstraction layer (Double tokens for offset, blur, opacity), and Shadows.xaml the effect provider, we maintain the separation of concerns. Controls that need `Effect="{DynamicResource Elevation.Card}"` will still work since the `Elevation.Card` key will reference `Shadow.Card` in Shadows.xaml.

**Implementation**: Add `Shadow.Card`, `Shadow.Popup`, `Shadow.Window` aliases in Shadows.xaml mapping to the same DropShadowEffect instances. Keep `Elevation.Card` etc. as `DynamicResource` proxies pointing to `Shadow.*` keys.

Wait — WPF `DropShadowEffect` is a sealed class and cannot use `DynamicResource` for the inner properties. The `Effect` property itself CAN use `DynamicResource`. So the approach is:
1. Keep all DropShadowEffect definitions in `Effects/Shadows.xaml` (they already exist there as `Shadow.Card`, `Shadow.Popup`, etc.)
2. Remove the duplicate DropShadowEffect definitions from `Base/Elevation.xaml`
3. Convert `Base/Elevation.xaml` to contain only `sys:Double` tokens for elevation parameters (OffsetX, OffsetY, BlurRadius, Opacity) — these are abstraction values controls can reference for custom shadow composition
4. Controls use `Effect="{DynamicResource Shadow.Card}"` directly

**Alternatives considered**:
- Keep Elevation.xaml as-is in Base/: Rejected — violates Constitution XII.
- Delete Elevation.xaml entirely: Rejected — elevation abstraction values are useful for custom shadow composition.

## Research Task 5: Legacy Flat Key Deprecation Strategy

**Decision**: Add `<!-- DEPRECATED: Use Brush.Background.Primary instead. Removal: Phase 6 -->` XML comments above each legacy flat key in Colors.xaml. No code behavior change.

**Rationale**: XML comments are visible in IntelliSense and code review, don't affect runtime, and can be easily searched for during Phase 6 cleanup. WPF has no built-in `[Obsolete]` attribute for XAML resources, so comments are the standard approach.

**Alternatives considered**:
- Separate LegacyKeys.xaml file: Rejected — adds complexity, changes loading order, and existing references use flat keys everywhere.
- Remove immediately: Rejected — would break 35+ references across control styles and windows.
- No action: Rejected — spec FR-003 requires deprecation annotation.

## Research Task 6: Token Integrity Validation Approach (FR-010)

**Decision**: Two-pronged validation:
1. **Build-time lint**: PowerShell script (`scripts/Validate-Tokens.ps1`) that runs regex grep checks for inline hex, StaticResource on themeable properties, and inline DropShadowEffect. Returns exit code 0 for pass, 1 for violations. Integrated into build process.
2. **Runtime validation**: ThemeManager enhancement that logs `Debug.WriteLine` warnings when resolving a resource that doesn't exist in the current dictionary chain (using `Application.Current.Dispatcher.BeginInvoke` to check resource lookup after theme switch).

**Rationale**: Build-time catches violations before they reach runtime; runtime catches missing themes or broken chains during development.

**Alternatives considered**:
- Build-time only: Rejected — spec FR-010 requires both.
- Runtime only: Rejected — spec FR-010 requires both; also, runtime warnings are less actionable than build failures.
- XAML schema validation: Rejected — .NET Framework 4.8 WPF doesn't support XAML schema enforcement natively.

## Research Task 7: Primitive.Transparency Tokens for Derived States (FR-004 Clarification)

**Decision**: Add `Primitive.Transparency.*` Color tokens to Colors.xaml for the semi-transparent alpha values used in derived state tokens. These tokens represent alpha channel values, not standalone colors. Semantic `Brush.*` tokens for derived states (hover, pressed) compose from `Primitive.*` base color + `Primitive.Transparency.*` alpha.

**Rationale**: The clarification specified that derived state tokens compose from Primitive base color + Primitive transparency. WPF `SolidColorBrush` doesn't support color composition at the token level, so these tokens are resolved as hexadecimal Color values with pre-composed alpha (e.g., `Color.Button.Background = #2FFFFFFF = Primitive.White.20 at 18% opacity`). The `Primitive.Transparency.*` tokens serve as documentation and validation checkpoints.

**Implementation**: Add transparency tokens to document the alpha channel mappings:
```xml
<!-- Transparency tokens (documentation/validation) -->
<Color x:Key="Primitive.Transparency.Subtle">0DFFFFFF</Color>  <!-- 5% white -->
<Color x:Key="Primitive.Transparency.Light">1AFFFFFF</Color>  <!-- 10% white -->
<Color x:Key="Primitive.Transparency.MediumLow">1FFFFFFF</Color>  <!-- 12.5% white -->
<Color x:Key="Primitive.Transparency.Medium">2FFFFFFF</Color>  <!-- ~18% white -->
<Color x:Key="Primitive.Transparency.Strong">3FFFFFFF</Color>  <!-- ~25% white -->
<Color x:Key="Primitive.Transparency.Heavy">44007ACC</Color>  <!-- selected-state accent -->
```

These tokens make the alpha composition explicit and auditable without changing the underlying hex values in the `Color.*` semantic tokens.

**Alternatives considered**:
- No transparency tokens, just document in comments: Rejected — spec FR-004 requires Primitive.Transparency composition.
- Runtime alpha composition: Rejected — WPF SolidColorBrush doesn't support runtime alpha composition from two DynamicResource references.