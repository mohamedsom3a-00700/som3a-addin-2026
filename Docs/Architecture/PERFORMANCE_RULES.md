# Performance Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Target**: Excel VSTO-hosted WPF, .NET Framework 4.8

---

## 1. Rendering Budget

### DropShadowEffect

- Maximum **one** per visual element
- Nested shadows are **prohibited**
- Use centralized keys from `Effects/Shadows.xaml`
- In safe mode: use `Shadow.*.Safe` variants (lower blur/opacity)

### BlurEffect

- **Prohibited** on scrollable containers
- **Prohibited** on items with frequent updates
- Allowed only on static decorative elements (backgrounds, overlays)

### Opacity Animations

- Preferred over layout animations
- Use `Opacity` on containers, not individual children

---

## 2. Layout Budget

### Avoid Runtime Layout Invalidation

- Minimize `Width`/`Height` changes that trigger parent re-layout
- Use `ScaleTransform` for hover effects instead of size changes
- Use `TranslateTransform` for movement instead of margin changes

### Virtualization

| Control | Requirement |
|---------|-------------|
| DataGrid | `EnableRowVirtualization="True"` |
| DataGrid | `EnableColumnVirtualization="True"` |
| DataGrid | `VirtualizingPanel.VirtualizationMode="Recycling"` |
| ListView | `VirtualizingPanel.IsVirtualizing="True"` |
| ListView | `VirtualizingPanel.VirtualizationMode="Recycling"` |
| TreeView | `VirtualizingPanel.IsVirtualizing="True"` |

### List Thresholds

- Lists with **50+ items** MUST use virtualization
- Lists with **200+ items** SHOULD use `Recycling` mode
- Lists with **1000+ items** MUST use `Recycling` mode + `IsDeferredScrollingEnabled`

---

## 3. Memory Budget

### Brush Sharing

- Brushes MUST be reusable and shared globally
- Do not create unique brushes per instance
- Use `DynamicResource` to share brush references

### Resource Caching

- `WindowRenderModeDetector` result MUST be cached per session
- Theme dictionary loading result SHOULD be cached
- Avoid re-parsing ResourceDictionary on every access

### Window Lifecycle

- Reuse windows where possible (singleton dialogs)
- Close and nullify references when no longer needed
- Avoid keeping closed windows in memory

---

## 4. Animation Budget

| Animation Type | Max Duration | Safe Mode |
|----------------|-----------|-----------|
| Hover | 150ms | Skip |
| Press | 50ms | Skip |
| Release | 80ms | Skip |
| Theme switch | 200ms | Skip |
| Window open | 200ms | Instant |
| Window close | 150ms | Instant |
| Popup open | 150ms | Instant |
| Popup close | 100ms | Instant |

### GPU Impact

- `ScaleTransform` — GPU-friendly ✅
- `Opacity` — GPU-friendly ✅
- `TranslateTransform` — GPU-friendly ✅
- `BlurEffect` — GPU-heavy ⚠️
- `DropShadowEffect` — Moderate GPU impact ⚠️

---

## 5. Excel Host Specific

### Transparency

- `AllowsTransparency="False"` preferred
- If `True` required, test in Excel host before merge
- FallbackSafe mode forces `False`

### Window Chrome

- Use `WindowChrome` when safe
- Fallback to standard chrome when `WindowRenderModeDetector` says so
- Never leave window without chrome in Excel host

### Focus Management

- Return focus to Excel when window closes
- Handle `PreviewKeyDown` for Escape-to-close
- Do not block Excel interactivity longer than necessary

---

## 6. Performance Validation Checklist

Before merge, verify:

- [ ] No nested DropShadowEffect
- [ ] No BlurEffect on scrollable containers
- [ ] Virtualization enabled on large lists
- [ ] Animations ≤ 200ms
- [ ] No layout-thrashing animations
- [ ] Brushes shared via DynamicResource
- [ ] Safe mode reduces effects appropriately
- [ ] Memory does not grow on repeated open/close

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [TOKEN_RULES.md](TOKEN_RULES.md) | Token naming conventions |
| [EXCEL_RENDERING_RULES.md](EXCEL_RENDERING_RULES.md) | Excel VSTO rendering safety |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
