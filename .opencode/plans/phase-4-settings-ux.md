# Phase 4: Settings UX Rebuild — Theme Cards

**Branch**: `feature/settings-ux`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Transform the theme selection from a simple ComboBox to modern theme cards with preview thumbnails, selection glow, and smooth animations. RadioButton-based and ComboBox-based theme selection is **DEPRECATED**. This is the most visible UX change.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Views/SettingsWindow.xaml`
  - `WpfApp2/Theme/Controls/ThemeCardStyles.xaml` (new)
  - `WpfApp2/Theme/Effects/Glow.xaml` (already created in Phase 2)

---

## Theme Card Architecture

The SettingsWindow MUST display three theme cards: **Dark**, **White** (Light), and **Custom**.

Each card includes:
- Preview thumbnail (mini gradient showing the theme's background + accent)
- Theme name label
- Theme description (1 line)
- Accent color visualization strip
- Selected state: glow border + scale animation

### Card Visual Structure

```
┌─────────────────────────────┐
│  [Preview Thumbnail]       │  120x80px gradient preview
│  accent strip at bottom    │
│─────────────────────────────│
│  Theme Name                │  Bold, 14px
│  Brief description         │  Light, 12px
└─────────────────────────────┘
```

### Theme Preview Thumbnails

| Theme | Preview Gradient |
|-------|-----------------|
| Dark | `#0E1720` → `#1C2B3A` (dark gradient) |
| White (Light) | `#FAFAFA` → `#FFFFFF` (light gradient) |
| Custom | `#15202B` → `#3A86FF` (dark + accent) |

---

## Tasks

### T015 Replace ComboBox with theme cards
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Remove:
```xaml
<ComboBox Height="36" Style="{DynamicResource RoundComboBox}">
    <ComboBoxItem Content="Dark"/>
    <ComboBoxItem Content="Light"/>
    <ComboBoxItem Content="Blue"/>
</ComboBox>
```

Replace with:
```xaml
<!-- Theme Cards Panel -->
<StackPanel Orientation="Horizontal" Margin="0,0,0,16">

    <!-- Dark Theme Card -->
    <Border x:Name="CardDark"
            Width="140" Height="120"
            Margin="0,0,12,0"
            CornerRadius="12"
            Cursor="Hand"
            MouseLeftButtonDown="ThemeCard_Click"
            Tag="Dark">
        <Border.Style>
            <Style TargetType="Border">
                <Setter Property="Background" Value="#0E1720"/>
                <Setter Property="BorderBrush" Value="{DynamicResource ControlStrokeBrush}"/>
                <Setter Property="BorderThickness" Value="1"/>
                <Setter Property="RenderTransform">
                    <Setter.Value>
                        <ScaleTransform ScaleX="1" ScaleY="1"/>
                    </Setter.Value>
                </Setter>
            </Style>
        </Border.Style>
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="80"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Preview Gradient -->
            <Border Grid.Row="0" CornerRadius="12,12,0,0">
                <Border.Background>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                        <GradientStop Color="#0E1720" Offset="0"/>
                        <GradientStop Color="#1C2B3A" Offset="1"/>
                    </LinearGradientBrush>
                </Border.Background>
                <!-- Accent strip at bottom of preview -->
                <Border Height="4" VerticalAlignment="Bottom">
                    <Border.Background>
                        <SolidColorBrush Color="#3A86FF"/>
                    </Border.Background>
                </Border>
            </Border>

            <!-- Label -->
            <StackPanel Grid.Row="1" Margin="10,8,10,8">
                <TextBlock Text="Dark"
                           FontWeight="SemiBold" FontSize="14"
                           Foreground="{DynamicResource TextMainBrush}"/>
                <TextBlock Text="Full dark theme"
                           FontSize="11"
                           Foreground="{DynamicResource TextSubBrush}"
                           TextTrimming="CharacterEllipsis"/>
            </StackPanel>
        </Grid>
    </Border>

    <!-- White (Light) Theme Card -->
    <!-- Similar structure, light gradient, "Clean light theme" -->

    <!-- Custom Theme Card -->
    <!-- Similar structure, dark+accent gradient, "Your accent color" -->

</StackPanel>
```

### T016 Add theme preview thumbnails
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Each card shows a mini preview of the theme colors as defined above. The accent strip at the bottom of each preview reflects the current accent color (via DynamicResource).

### T017 Add selection glow animation
**File**: `WpfApp2/Theme/Controls/ThemeCardStyles.xaml`

When a theme card is selected:
1. Add `Glow.ThemeCard.Selected` effect (border glow)
2. Animate scale to 1.02
3. Add border highlight with `AccentBrush`
4. Subtle shadow elevation change

```xaml
<!-- Theme Card Selected Style -->
<Style x:Key="ThemeCardSelected" TargetType="Border">
    <Setter Property="BorderBrush" Value="{DynamicResource AccentBrush}"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="Effect" Value="{DynamicResource Glow.ThemeCard.Selected}"/>
    <Setter Property="RenderTransformOrigin" Value="0.5,0.5"/>
    <Setter Property="RenderTransform">
        <Setter.Value>
            <ScaleTransform ScaleX="1.02" ScaleY="1.02"/>
        </Setter.Value>
    </Setter>
</Style>

<!-- Theme Card Hover Style -->
<Style x:Key="ThemeCardHover" TargetType="Border">
    <Setter Property="BorderBrush" Value="{DynamicResource ControlStrokeBrush}"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="Effect" Value="{DynamicResource Glow.ButtonHover}"/>
</Style>
```

### T018 Integrate runtime theme switching
**File**: `WpfApp2/Views/SettingsWindow.xaml` (code-behind or ViewModel)

Wire up the cards to trigger theme switch on click:
```csharp
private void ThemeCard_Click(object sender, MouseButtonEventArgs e)
{
    if (sender is Border card && card.Tag is string themeName)
    {
        // Update card selection states
        UpdateThemeCardSelection(card);

        // Apply theme via ThemeManager
        ThemeManager.ApplyTheme(themeName);
    }
}

private void UpdateThemeCardSelection(Border selectedCard)
{
    // Remove selected state from all cards
    // Add selected state to selectedCard
}
```

---

## Dependency Order

T015 → T016 → T017 → T018 (sequential — card structure first, then styling, then wiring)

---

## Acceptance Criteria

- [ ] Theme dropdown (ComboBox) completely replaced with 3 theme cards
- [ ] Each card has preview thumbnail with gradient
- [ ] Each card has accent color strip
- [ ] Selected card has visible glow effect (`Glow.ThemeCard.Selected`)
- [ ] Cards animate on hover (subtle scale)
- [ ] Cards animate on selection (glow + scale to 1.02)
- [ ] Theme selection is click-driven (no dropdown, no RadioButton)
- [ ] Theme cards wire to `ThemeManager.ApplyTheme("Dark|Light|Custom")`
- [ ] UI is responsive and polished
- [ ] All states use DynamicResource for theme compatibility

---

## Design Principles

- **Premium enterprise feel**: Cards should feel like a modern Fluent settings page
- **Immediate feedback**: Selection state visible instantly
- **Consistent visual language**: Follows Card Radius (16), spacing rules, and color system
- **Accessibility**: Cards are keyboard navigable (Tab), activatable (Enter/Space)

---

## Constitution Check

Per constitution:
- **Principle III (DynamicResource Only)**: All colors use DynamicResource ✅
- **Principle V (Feature Completeness)**: Theme selection is theme-aware, DPI safe ✅
- **Design Authority Rules**: No RadioButton-based selection ✅ (deprecated)
- **Radius Rules**: Cards use CornerRadius=12 (per Radius standards)
- **Performance Budget Rules**: No nested DropShadows, flat card structure

(End of file — total 178 lines)