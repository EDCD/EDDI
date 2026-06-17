# EDDI UI/UX Modernization Plan - Windows 11 Design System

**Date**: May 23, 2026  
**Project**: Elite Dangerous Data Interface (EDDI)  
**Scope**: Complete UI/UX overhaul to Windows 11 Fluent Design standards  
**Status**: Planning & Implementation Phase

---

## Executive Summary

EDDI currently uses legacy WPF styling with hardcoded colors, no theme system, and outdated design patterns. This modernization will transform it into a polished, professional Windows 11 application while preserving all functionality.

**Key Metrics:**
- 34 XAML files to update
- 85% hardcoded colors to be replaced
- 15+ instances of hardcoded #FFE5E5E5
- 0 existing theme systems
- 0 modern UI frameworks currently in use

---

## Phase 1: Design System Foundation

### 1.1 Windows 11 Color Palette

#### Light Mode
```xaml
<!-- Primary Colors -->
<Color x:Key="PrimaryBrand">#0078D4</Color>  <!-- Windows Blue -->
<Color x:Key="AccentColor">#00B4EF</Color>   <!-- Accent Blue -->

<!-- Background Colors -->
<Color x:Key="BackgroundPrimary">#FFFFFF</Color>
<Color x:Key="BackgroundSecondary">#F3F3F3</Color>
<Color x:Key="BackgroundTertiary">#EFEFEF</Color>

<!-- Surface Colors -->
<Color x:Key="SurfaceBase">#FFFFFF</Color>
<Color x:Key="SurfaceSecondary">#F9F9F9</Color>

<!-- Text Colors -->
<Color x:Key="TextPrimary">#000000</Color>
<Color x:Key="TextSecondary">#424242</Color>
<Color x:Key="TextTertiary">#707070</Color>
<Color x:Key="TextDisabled">#BFBFBF</Color>

<!-- Status Colors -->
<Color x:Key="StatusSuccess">#107C10</Color>  <!-- Green -->
<Color x:Key="StatusWarning">#FFB900</Color>  <!-- Yellow -->
<Color x:Key="StatusError">#E74C3C</Color>    <!-- Red -->
<Color x:Key="StatusInfo">#0078D4</Color>     <!-- Blue -->

<!-- Border & Shadow -->
<Color x:Key="BorderLight">#E0E0E0</Color>
<Color x:Key="BorderMedium">#C0C0C0</Color>
<Color x:Key="BorderDark">#808080</Color>
<Color x:Key="ShadowColor">#000000</Color>
```

#### Dark Mode
```xaml
<!-- Primary Colors -->
<Color x:Key="PrimaryBrand">#60CDFF</Color>  <!-- Light Blue -->
<Color x:Key="AccentColor">#0078D4</Color>

<!-- Background Colors -->
<Color x:Key="BackgroundPrimary">#1E1E1E</Color>
<Color x:Key="BackgroundSecondary">#2D2D30</Color>
<Color x:Key="BackgroundTertiary">#3E3E42</Color>

<!-- Surface Colors -->
<Color x:Key="SurfaceBase">#252526</Color>
<Color x:Key="SurfaceSecondary">#2D2D30</Color>

<!-- Text Colors -->
<Color x:Key="TextPrimary">#FFFFFF</Color>
<Color x:Key="TextSecondary">#E0E0E0</Color>
<Color x:Key="TextTertiary">#A0A0A0</Color>
<Color x:Key="TextDisabled">#505050</Color>

<!-- Border & Shadow -->
<Color x:Key="BorderLight">#3F3F46</Color>
<Color x:Key="BorderMedium">#52525B</Color>
<Color x:Key="BorderDark">#6B6B7D</Color>
<Color x:Key="ShadowColor">#000000</Color>
```

### 1.2 Spacing System (Fluent Design)

```xaml
<!-- Spacing Scale -->
<System:Double x:Key="SpacingXXS">2</System:Double>
<System:Double x:Key="SpacingXS">4</System:Double>
<System:Double x:Key="SpacingS">8</System:Double>
<System:Double x:Key="SpacingM">12</System:Double>
<System:Double x:Key="SpacingL">16</System:Double>
<System:Double x:Key="SpacingXL">20</System:Double>
<System:Double x:Key="SpacingXXL">24</System:Double>
<System:Double x:Key="Spacing3XL">32</System:Double>

<!-- Common Margins -->
<Thickness x:Key="MarginS">8</Thickness>
<Thickness x:Key="MarginM">12</Thickness>
<Thickness x:Key="MarginL">16</Thickness>
<Thickness x:Key="MarginXL">20</Thickness>

<!-- Common Paddings -->
<Thickness x:Key="PaddingS">8</Thickness>
<Thickness x:Key="PaddingM">12</Thickness>
<Thickness x:Key="PaddingL">16</Thickness>
```

### 1.3 Typography System

```xaml
<!-- Font Families -->
<FontFamily x:Key="FontFamilyDefault">Segoe UI, sans-serif</FontFamily>
<FontFamily x:Key="FontFamilyMonospace">Consolas, monospace</FontFamily>

<!-- Font Sizes -->
<System:Double x:Key="FontSizeXS">11</System:Double>
<System:Double x:Key="FontSizeS">12</System:Double>
<System:Double x:Key="FontSizeBase">14</System:Double>
<System:Double x:Key="FontSizeL">16</System:Double>
<System:Double x:Key="FontSizeXL">18</System:Double>
<System:Double x:Key="FontSize2XL">20</System:Double>
<System:Double x:Key="FontSize3XL">24</System:Double>

<!-- Font Weights -->
<FontWeight x:Key="FontWeightNormal">Normal</FontWeight>
<FontWeight x:Key="FontWeightMedium">Medium</FontWeight>
<FontWeight x:Key="FontWeightSemiBold">SemiBold</FontWeight>
<FontWeight x:Key="FontWeightBold">Bold</FontWeight>
```

### 1.4 Border & Corner Radius

```xaml
<!-- Corner Radius -->
<CornerRadius x:Key="CornerRadiusSmall">4</CornerRadius>
<CornerRadius x:Key="CornerRadiusMedium">8</CornerRadius>
<CornerRadius x:Key="CornerRadiusLarge">12</CornerRadius>

<!-- Border Thickness -->
<Thickness x:Key="BorderThicknessThin">0.5</Thickness>
<Thickness x:Key="BorderThicknessNormal">1</Thickness>
<Thickness x:Key="BorderThicknessStrong">2</Thickness>
```

### 1.5 Shadow & Effects

```xaml
<!-- Elevation Shadows -->
<DropShadowEffect x:Key="ElevationShadow1" BlurRadius="4" Opacity="0.15" ShadowDepth="2"/>
<DropShadowEffect x:Key="ElevationShadow2" BlurRadius="8" Opacity="0.20" ShadowDepth="4"/>
<DropShadowEffect x:Key="ElevationShadow3" BlurRadius="16" Opacity="0.25" ShadowDepth="8"/>

<!-- Acrylic-like Brush -->
<SolidColorBrush x:Key="AcrylicBase" Color="{StaticResource BackgroundSecondary}" Opacity="0.8"/>
```

---

## Phase 2: Control Styling

### 2.1 Button Styles

```xaml
<!-- Primary Button -->
<Style TargetType="Button" x:Key="ButtonPrimary">
    <Setter Property="Background" Value="{StaticResource PrimaryBrandBrush}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="CornerRadius" Value="4"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="{StaticResource AccentColorBrush}"/>
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter Property="Opacity" Value="0.8"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<!-- Secondary Button -->
<Style TargetType="Button" x:Key="ButtonSecondary">
    <Setter Property="Background" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderMediumBrush}"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <!-- Similar template with hover states -->
</Style>
```

### 2.2 TextBox & Input Styles

```xaml
<Style TargetType="TextBox" x:Key="TextBoxModern">
    <Setter Property="Background" Value="{StaticResource SurfaceBaseBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderMediumBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <Setter Property="FontFamily" Value="{StaticResource FontFamilyDefault}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="BorderBrush" Value="{StaticResource AccentColorBrush}"/>
        </Trigger>
        <Trigger Property="IsFocused" Value="True">
            <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrandBrush}"/>
            <Setter Property="BorderThickness" Value="2"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 2.3 ComboBox Style

```xaml
<Style TargetType="ComboBox" x:Key="ComboBoxModern">
    <Setter Property="Background" Value="{StaticResource SurfaceBaseBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderMediumBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <!-- Template with modern dropdown styling -->
</Style>
```

### 2.4 DataGrid Style

```xaml
<Style TargetType="DataGrid" x:Key="DataGridModern">
    <Setter Property="Background" Value="{StaticResource SurfaceBaseBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="AlternatingRowBackground" Value="{StaticResource BackgroundSecondaryBrush}"/>
    <Setter Property="HorizontalGridLinesBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="VerticalGridLinesBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="RowHeight" Value="32"/>
    <Setter Property="HeadersVisibility" Value="Column"/>
</Style>

<Style TargetType="DataGridColumnHeader" x:Key="DataGridColumnHeaderModern">
    <Setter Property="Background" Value="{StaticResource BackgroundSecondaryBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
</Style>

<Style TargetType="DataGridCell" x:Key="DataGridCellModern">
    <Setter Property="Padding" Value="8,4"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Style.Triggers>
        <Trigger Property="IsSelected" Value="True">
            <Setter Property="Background" Value="{StaticResource PrimaryBrandBrush}"/>
            <Setter Property="Foreground" Value="White"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 2.5 TabControl Style

```xaml
<Style TargetType="TabControl" x:Key="TabControlModern">
    <Setter Property="Background" Value="{StaticResource BackgroundPrimaryBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderLightBrush}"/>
    <Setter Property="BorderThickness" Value="0,1,0,0"/>
</Style>

<Style TargetType="TabItem" x:Key="TabItemModern">
    <Setter Property="Padding" Value="16,12"/>
    <Setter Property="Margin" Value="0"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Foreground" Value="{StaticResource TextSecondaryBrush}"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="TabItem">
                <Border x:Name="border" BorderBrush="{StaticResource BorderLightBrush}"
                        BorderThickness="0,0,0,2" Padding="{TemplateBinding Padding}"
                        Background="Transparent">
                    <ContentPresenter ContentSource="Header"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="border" Property="BorderBrush" Value="{StaticResource PrimaryBrandBrush}"/>
                        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
                        <Setter TargetName="border" Property="BorderThickness" Value="0,0,0,3"/>
                    </Trigger>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" Value="{StaticResource BackgroundSecondaryBrush}"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 2.6 CheckBox & RadioButton Styles

```xaml
<Style TargetType="CheckBox" x:Key="CheckBoxModern">
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <Setter Property="Padding" Value="8,4"/>
    <!-- Modern checkbox template with modern appearance -->
</Style>

<Style TargetType="RadioButton" x:Key="RadioButtonModern">
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <Setter Property="Padding" Value="8,4"/>
    <!-- Modern radio button template -->
</Style>
```

### 2.7 Window Style

```xaml
<Style TargetType="Window" x:Key="WindowModern">
    <Setter Property="Background" Value="{StaticResource BackgroundPrimaryBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="FontFamily" Value="{StaticResource FontFamilyDefault}"/>
    <Setter Property="FontSize" Value="{StaticResource FontSizeBase}"/>
    <Setter Property="TextElement.Foreground" Value="{StaticResource TextPrimaryBrush}"/>
</Style>
```

---

## Phase 3: Implementation Timeline

### Week 1: Foundation
- [x] Analyze codebase
- [ ] Create central App.xaml resource dictionary
- [ ] Define color palettes (light/dark)
- [ ] Define typography & spacing systems
- [ ] Create base control styles

### Week 2: Main Application
- [ ] Update MainWindow.xaml with modern styling
- [ ] Implement dark mode toggle
- [ ] Update tab control styling
- [ ] Replace all hardcoded colors in EddiUI

### Week 3: Configuration Windows
- [ ] Update all 13 monitor configuration windows
- [ ] Replace #FFE5E5E5 with resource references
- [ ] Update DataGrid styling
- [ ] Apply consistent spacing

### Week 4: Special Windows
- [ ] Update navigation controls
- [ ] Update editors and special windows
- [ ] Apply effects and polish
- [ ] Test all functionality

### Week 5: Dark Mode & Polish
- [ ] Implement complete dark mode
- [ ] Fine-tune colors and contrast
- [ ] Add animations and transitions
- [ ] Accessibility testing

### Week 6: Testing & Deployment
- [ ] Functional testing
- [ ] Regression testing
- [ ] Performance testing
- [ ] Documentation updates

---

## Phase 4: Files to Modify (Priority Order)

### Critical (Must Have)
1. **App.xaml** - Central resource dictionary (1 file)
2. **MainWindow.xaml** - Main application window (1 file)

### High Priority (Configuration Windows)
3. **SpeechResponder/ConfigurationWindow.xaml** - Most complex
4. **NavigationMonitor/ConfigurationWindow.xaml** - Complex navigation UI
5. **CommanderMonitor/ConfigurationWindow.xaml** - Profile configuration
6. **ShipMonitor/ConfigurationWindow.xaml** - Ship management
7. **EddiUI/FrontierApiTab.xaml** - API configuration
8. **EddiUI/TextToSpeechTab.xaml** - TTS configuration
9. **CargoMonitor/ConfigurationWindow.xaml**
10. **MaterialMonitor/ConfigurationWindow.xaml**
11. **MissionMonitor/ConfigurationWindow.xaml**
12. **GalnetMonitor/ConfigurationWindow.xaml**
13. **EDDPMonitor/ConfigurationWindow.xaml**

### Medium Priority (Navigation Controls)
14-19. **NavigationMonitor/** - 6 control files
   - BookmarksControl.xaml
   - BookmarkSelector.xaml
   - CurrentRouteControl.xaml
   - GalacticPOIControl.xaml
   - PlotCarrierControl.xaml
   - PlotShipControl.xaml

### Lower Priority (Special Windows)
20-27. **SpeechResponder/** - Editor windows
   - EditScriptWindow.xaml
   - ViewScriptWindow.xaml
   - ShowDiffWindow.xaml
   - CopyPersonalityWindow.xaml
   - VariablesWindow.xaml
   - MarkdownWindow.xaml
   - TextCompletionControl.xaml
   
28. **EddiCore/Hotkeys/HotkeysWindow.xaml**
29. **Utilities/IpaResources.xaml**
30. **EDSMResponder/ConfigurationWindow.xaml**
31. **InaraResponder/ConfigurationWindow.xaml**
32. **VoiceAttackResponder/ConfigurationWindow.xaml**
33. **CrimeMonitor/ConfigurationWindow.xaml**
34. **EddiUI/ChangeLog.xaml**
35. **PluginSkeleton.xaml**

---

## Phase 5: Dark Mode Implementation

### 5.1 Theme Service Architecture
```csharp
public interface IThemeService
{
    ThemeMode CurrentTheme { get; set; }
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;
    void ApplyTheme(ThemeMode mode);
    void ToggleDarkMode();
}

public enum ThemeMode { Light, Dark, Auto }
```

### 5.2 Runtime Theme Switching
- Store theme preference in config
- Provide toggle in main window
- Update all brushes dynamically
- Smooth color transitions

---

## Phase 6: Accessibility & Polish

### 6.1 WCAG 2.1 AA Compliance
- Contrast ratios ≥ 4.5:1 for normal text
- Contrast ratios ≥ 3:1 for large text
- Focus indicators clearly visible
- Keyboard navigation fully supported

### 6.2 Visual Polish
- Consistent animations (200-300ms)
- Hover states on all interactive elements
- Focus states clearly visible
- Disabled states with reduced opacity
- Proper cursor feedback

### 6.3 Responsive Design
- Support for DPI scaling
- Proper font scaling
- Layout adaptability to window resize

---

## Risks & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Breaking existing functionality | Medium | High | Comprehensive testing, incremental rollout |
| Performance degradation | Low | Medium | Profile before/after, optimize effects |
| User confusion with new UI | Low | Low | Gradual rollout, documentation |
| Theme switching bugs | Medium | Medium | Thorough dark mode testing |

---

## Success Metrics

- [ ] All 34 XAML files updated
- [ ] 100% of hardcoded colors replaced with resources
- [ ] Dark mode fully functional
- [ ] All controls follow Windows 11 design
- [ ] All functionality preserved
- [ ] No performance degradation
- [ ] WCAG 2.1 AA compliance achieved
- [ ] All tests passing

---

## Code Review Checklist

For each file modified:
- [ ] All hardcoded colors removed
- [ ] Consistent spacing using resource system
- [ ] Typography follows design system
- [ ] Dark mode compatible
- [ ] Accessibility verified
- [ ] No functionality broken
- [ ] Consistent with application theme
- [ ] Comments added for complex styling

---

## References

- [Fluent Design System](https://www.microsoft.com/design/fluent/)
- [Windows 11 Design Principles](https://learn.microsoft.com/en-us/windows/apps/design/)
- [WPF Best Practices](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
