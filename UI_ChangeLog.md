# EDDI UI/UX Modernization & Theme Selector Change Log

This document details all the modifications, enhancements, and bug fixes introduced during the UI/UX overhaul of the Elite Dangerous Data Interface (EDDI) desktop application.

---

## 1. Visual Theme Modernization
* **Centralized Design System**: Added three core styling dictionaries to clean up legacy styles:
  * [ThemeLight.xaml](file:///f:/EDDI-develop/EddiUI/Themes/ThemeLight.xaml): Establishes premium Fluent Light Mode tokens (typography, margins, borders, and harmonious backgrounds).
  * [ThemeDark.xaml](file:///f:/EDDI-develop/EddiUI/Themes/ThemeDark.xaml): Provides identical dark mode token counterparts to support visual comfort.
  * [ThemeModern.xaml](file:///f:/EDDI-develop/EddiUI/Themes/ThemeModern.xaml): Defines global Fluent-style control templates with rounded corners (`CornerRadius="4"`), hover/focused transitions, and consistent padding.
* **Control Template Overhauls**: Modernized primary layout elements across the app:
  * **Window**: Cleaned up default visual chrome.
  * **Buttons**: Rounded styling with responsive click/hover states.
  * **TextBoxes & ComboBoxes**: Added active bottom accent lines and rounded borders on focus.
  * **TabControl & TabItem**: Shifted to a flat header list layout with active accent lines.
  * **CheckBox & RadioButton**: Designed custom vector-based Fluent marks that replace the default Win98-style controls.

---

## 2. Dynamic Accent and Theme Tracking
* **Automated Theme Detection**: Added [ThemeManager.cs](file:///f:/EDDI-develop/EddiUI/Themes/ThemeManager.cs) to query the Windows registry (`AppsUseLightTheme`) and automatically detect whether light or dark theme is active.
* **Accent Color Integration**: Extracted the user's personal Windows Colorization/Accent settings from the registry or system glass parameters, dynamically updating `PrimaryBrandBrush` in memory.
* **Runtime Event Binding**: Hooked into `Microsoft.Win32.SystemEvents.UserPreferenceChanged` to apply theme and accent shifts instantly in real-time without requiring application restarts.

---

## 3. Localized User-Selectable Theme Dropdown
* **Layout Addition**: Added the theme selector dropdown (`chooseThemeDropDown`) to the topmost EDDI configuration tab directly below the language selector.
* **String Localization**: Added resource keys (`choose_theme_label`, `choose_theme_label_va`, `theme_system`, `theme_light`, `theme_dark`) to `Resources.resx` and exposed them strongly-typed via `Resources.Designer.cs`.
* **ThemeDef Model**: Introduced a `ThemeDef` mapping structure in `MainWindow.xaml.cs` to pair config identifiers (`System`, `Light`, `Dark`) with their localized text descriptions, ensuring proper display and language-independent persistence.

---

## 4. AvalonEdit Script Editor High-Contrast Theme
* **Syntax Highlighting**: Refactored the syntax coloring rules in `CottleHighlighting.cs` to dynamically swap high-contrast code coloring depending on the active theme.
* **Palette Choices**: Configured a modern VS Code / Antigravity styled theme for dark mode:
  * **Spoken Text / Plain Body**: Light Gray (`#D4D4D4`) for high contrast on `#262626` background.
  * **Keywords**: Vibrant Blue (`#569CD6`).
  * **Strings / Quotes**: Peach/Terracotta (`#CE9178`) - deuteranopia/protanopia friendly.
  * **Functions (Built-in & Custom)**: Warm Gold (`#DCDCAA`) and Cyan (`#4FC1FF`).
  * **Literals & Properties**: Light Olive (`#B5CEA8`) and Soft Cyan (`#9CDCFE`).
  * **Comments**: Highly readable light green (`#6A9955`).

---

## 5. Major UI Bug Fixes
* **ComboBox Internal Code Display**: Added missing `SelectionBoxItemTemplateSelector` binding in the ComboBox's selected item `ContentPresenter` in `ThemeModern.xaml`. This forces the selection box to display the human-readable string (via `DisplayMemberPath`) instead of the internal code/class type names.
* **Material Monitor Grade Icons**: Replaced hardcoded black vectors in `ConfigurationWindow.xaml` grade drawings with the dynamic `TextPrimaryBrush` resource, making the icons fully readable in both dark and light modes.
* **Speech Responder Styles**: Updated the row-level Priority ComboBox and Reset Button inline styles in `SpeechResponder/ConfigurationWindow.xaml` to use `BasedOn` style inheritance, preventing them from falling back to default legacy Windows Aero styling.

---

## 6. Build and Installer Enhancements
* **Absolute Path Compilers**: Updated `build-installer.cmd` to utilize absolute path mappings for Inno Setup 6 (`ISCC.exe`) and 7-Zip (`7z.exe`), preventing environment variable path failures.
