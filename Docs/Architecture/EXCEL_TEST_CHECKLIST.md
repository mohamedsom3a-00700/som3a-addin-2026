# Excel VSTO Test Checklist

**Purpose**: Manual test scenarios for verifying window rendering, DPI scaling, popup behavior, theme switching, and safe mode activation inside Excel VSTO host.

**Created**: 2026-05-22

---

## 1. Window Opening — Basic Rendering

- [ ] Open MainWindow from Excel VSTO ribbon — window renders without black screen
- [ ] Open each secondary window (14 total) — all render correctly with expected backgrounds
- [ ] Minimize and restore each window — no rendering artifacts after restore
- [ ] Maximize and restore each window — chrome adapts correctly
- [ ] Close each window — no freeze or unhandled exception

## 2. DPI Scaling

- [ ] Set system DPI to 100% — all windows scale correctly, no clipping
- [ ] Set system DPI to 125% — all windows scale correctly, no clipping or overlap
- [ ] Set system DPI to 150% — all windows scale correctly, proportional layout
- [ ] Verify DpiHelper.GetCurrentDpiScale() returns correct scale factor
- [ ] Verify DpiHelper.ScaleValue() produces proportional results

## 3. Popup Rendering

- [ ] Open ComboBox dropdown — popup renders without artifacts
- [ ] Open ContextMenu — renders with safe shadow if FallbackSafe mode
- [ ] Open Tooltip — renders correctly, no transparency artifacts in VSTO host
- [ ] Verify all popups use AllowsTransparency="False" in VSTO host

## 4. Theme Switching

- [ ] Switch from Dark to Light theme — all open windows update instantly
- [ ] Switch from Light to Dark theme — all open windows update instantly
- [ ] Apply custom accent color — all accent elements update without restart
- [ ] Verify no StaticResource usage on themeable properties (grep validation)

## 5. Safe Mode Activation

- [ ] Verify RenderModeService detects VSTO host and activates FallbackSafe
- [ ] In FallbackSafe mode — all animations are skipped (no fade in/out)
- [ ] In FallbackSafe mode — Shadow.*.Safe variants are applied instead of full shadows
- [ ] In FallbackSafe mode — AllowsTransparency is forced to False
- [ ] In FallbackSafe mode — backdrop effects are disabled

## 6. Window Navigation & Chrome

- [ ] Drag each window by title bar — moves correctly, no sticky behavior
- [ ] Resize each resizable window — chrome responds correctly
- [ ] Toggle minimize/maximize/close buttons — all work as expected
- [ ] Verify WindowChrome is applied when not in FallbackSafe mode

## 7. Shadow & Effects

- [ ] Verify zero inline DropShadowEffect outside Effects/ directory (grep validation)
- [ ] Verify Shadow.Window, Shadow.Card, Shadow.Popup resolve correctly
- [ ] Verify Shadow.Window.Safe, Shadow.Card.Safe, Shadow.Popup.Safe render with reduced blur
- [ ] Verify glow effects use DynamicResource AccentColorValue

## 8. Keyboard & Accessibility

- [ ] Tab through all interactive controls — focus indicator visible (Glow.Focus)
- [ ] Press Enter on focused button — activates button
- [ ] Press Escape on windows with CloseOnEscape — window closes
- [ ] Verify AutomationProperties.Name set on all interactive elements
- [ ] Verify high contrast mode awareness when system high contrast is active

---

## Pass Criteria

- All scenarios above pass without rendering artifacts
- No black windows observed in any scenario
- No Excel freezes during any window operation
- All themes switch correctly without restart