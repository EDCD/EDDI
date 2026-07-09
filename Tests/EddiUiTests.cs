using EddiUI;
using EddiUI.Themes;
using EddiSpeechService;
using EddiSpeechService.SpeechProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tests
{
    [TestClass, TestCategory("WpfTests"), DoNotParallelize]
    public class EddiUiTests : TestBase
    {
        [TestInitialize]
        public void Start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void LanguageDef_Constructor_WithCultureInfo_SetsDisplayName()
        {
            // Arrange
            var culture = new CultureInfo("en-US");

            // Act
            var languageDef = new MainWindow.LanguageDef(culture);

            // Assert
            Assert.IsNotNull(languageDef.displayName);
            Assert.Contains( culture.NativeName, languageDef.displayName );
            Assert.Contains( culture.DisplayName, languageDef.displayName );
            Assert.AreEqual(culture, languageDef.ci);
        }

        [TestMethod]
        public void LanguageDef_Constructor_WithCustomDisplayName_SetsCustomName()
        {
            // Arrange
            var culture = new CultureInfo("en-US");
            var customName = "Custom English";

            // Act
            var languageDef = new MainWindow.LanguageDef(culture, customName);

            // Assert
            Assert.AreEqual(customName, languageDef.displayName);
            Assert.AreEqual(culture, languageDef.ci);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_SortsAlphabetically()
        {
            // Arrange
            var langA = new MainWindow.LanguageDef(new CultureInfo("en"), "Aaa");
            var langB = new MainWindow.LanguageDef(new CultureInfo("fr"), "Bbb");
            var langC = new MainWindow.LanguageDef(new CultureInfo("de"), "Ccc");

            var languages = new List<MainWindow.LanguageDef> { langC, langA, langB };

            // Act
            languages.Sort();

            // Assert
            Assert.AreEqual("Aaa", languages[0].displayName);
            Assert.AreEqual("Bbb", languages[1].displayName);
            Assert.AreEqual("Ccc", languages[2].displayName);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_ReturnsZeroForEqualNames()
        {
            // Arrange
            var lang1 = new MainWindow.LanguageDef(new CultureInfo("en"), "Same");
            var lang2 = new MainWindow.LanguageDef(new CultureInfo("fr"), "Same");

            // Act
            var result = lang1.CompareTo(lang2);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_NegativeResult_WhenFirstIsLessThanSecond()
        {
            // Arrange
            var langA = new MainWindow.LanguageDef(new CultureInfo("en"), "Apple");
            var langZ = new MainWindow.LanguageDef(new CultureInfo("fr"), "Zebra");

            // Act
            var result = langA.CompareTo(langZ);

            // Assert
            Assert.IsLessThan( 0, result );
        }

        [TestMethod]
        public void LanguageDef_CompareTo_PositiveResult_WhenFirstIsGreaterThanSecond()
        {
            // Arrange
            var langZ = new MainWindow.LanguageDef(new CultureInfo("en"), "Zebra");
            var langA = new MainWindow.LanguageDef(new CultureInfo("fr"), "Apple");

            // Act
            var result = langZ.CompareTo(langA);

            // Assert
            Assert.IsGreaterThan(0, result );
        }

        [TestMethod]
        public void GetThemeDefs_IncludesExpectedThemes()
        {
            var expectedThemes = new[] { "System", "Light", "Dark", "Classic" };
            var themes = MainWindow.GetThemeDefs();

            CollectionAssert.AreEqual(
                expectedThemes,
                themes.Select(t => t.name).ToArray() );
        }

        [TestMethod]
        public void GetThemeDefs_UsesLocalizedDisplayNames()
        {
            var themes = MainWindow.GetThemeDefs();

            Assert.AreEqual(EddiUI.Properties.Resources.theme_system, themes.Single(t => t.name == "System").displayName);
            Assert.AreEqual(EddiUI.Properties.Resources.theme_light, themes.Single(t => t.name == "Light").displayName);
            Assert.AreEqual(EddiUI.Properties.Resources.theme_dark, themes.Single(t => t.name == "Dark").displayName);
            Assert.AreEqual(EddiUI.Properties.Resources.theme_classic, themes.Single(t => t.name == "Classic").displayName);
        }

        [TestMethod]
        public void GetSelectedThemeDef_UnknownThemeFallsBackToSystem()
        {
            var themes = MainWindow.GetThemeDefs();

            var selectedTheme = MainWindow.GetSelectedThemeDef(themes, "UnknownTheme");

            Assert.AreEqual("System", selectedTheme.name);
        }

        [STATestMethod]
        public void ApplyMainTabSizing_SetsMainTabDimensionsWithoutThemeStyleOverride()
        {
            var tabItem = new TabItem();

            MainWindow.ApplyMainTabSizing(tabItem);

            Assert.AreEqual(150, tabItem.MinWidth);
            Assert.AreEqual(30, tabItem.MinHeight);
        }

        [TestMethod]
        public void ResolveThemeDictionaryName_ClassicUsesClassicDictionary()
        {
            var dictionaryName = ThemeManager.ResolveThemeDictionaryName("Classic", false);

            Assert.AreEqual("ThemeClassic.xaml", dictionaryName);
        }

        [TestMethod]
        public void ShouldApplyWindowsAccentColor_ReturnsFalseForClassic()
        {
            Assert.IsFalse(ThemeManager.ShouldApplyWindowsAccentColor("Classic"));
            Assert.IsTrue(ThemeManager.ShouldApplyWindowsAccentColor("Light"));
            Assert.IsTrue(ThemeManager.ShouldApplyWindowsAccentColor("Dark"));
            Assert.IsTrue(ThemeManager.ShouldApplyWindowsAccentColor(null));
        }

        [TestMethod]
        public void ShouldReapplyThemeForUserPreferenceChange_IncludesThemeAndAccentChanges()
        {
            Assert.IsTrue(ThemeManager.ShouldReapplyThemeForUserPreferenceChange(UserPreferenceCategory.General));
            Assert.IsTrue(ThemeManager.ShouldReapplyThemeForUserPreferenceChange(UserPreferenceCategory.Color));
            Assert.IsFalse(ThemeManager.ShouldReapplyThemeForUserPreferenceChange(UserPreferenceCategory.Keyboard));
        }

        [TestMethod]
        public void ShouldUseDarkWindowFrame_ReturnsTrueOnlyForDarkThemeDictionary()
        {
            Assert.IsFalse(ThemeManager.ShouldUseDarkWindowFrame("ThemeLight.xaml"));
            Assert.IsTrue(ThemeManager.ShouldUseDarkWindowFrame("ThemeDark.xaml"));
            Assert.IsFalse(ThemeManager.ShouldUseDarkWindowFrame("ThemeClassic.xaml"));
        }

        [STATestMethod]
        public void ApplyWindowFrameForWindow_SkipsNullAndUninitializedWindows()
        {
            ThemeManager.ApplyWindowFrame( null );

            var window = new Window();

            ThemeManager.ApplyWindowFrame( window );
        }

        [TestMethod]
        public void ShouldLoadModernThemeDictionary_ReturnsFalseForClassic()
        {
            Assert.IsFalse(ThemeManager.ShouldLoadModernThemeDictionary("Classic"));
            Assert.IsTrue(ThemeManager.ShouldLoadModernThemeDictionary("Light"));
            Assert.IsTrue(ThemeManager.ShouldLoadModernThemeDictionary("Dark"));
            Assert.IsTrue(ThemeManager.ShouldLoadModernThemeDictionary(null));
        }

        [TestMethod]
        public void IsBaseThemeDictionarySource_IdentifiesBaseThemesButNotModernStyles()
        {
            Assert.IsTrue(ThemeManager.IsBaseThemeDictionarySource("/EddiUI;component/Themes/ThemeLight.xaml"));
            Assert.IsTrue(ThemeManager.IsBaseThemeDictionarySource("/EddiUI;component/Themes/ThemeDark.xaml"));
            Assert.IsTrue(ThemeManager.IsBaseThemeDictionarySource("/EddiUI;component/Themes/ThemeClassic.xaml"));
            Assert.IsFalse(ThemeManager.IsBaseThemeDictionarySource("/EddiUI;component/Themes/ThemeModern.xaml"));
            Assert.IsTrue(ThemeManager.IsModernThemeDictionarySource("/EddiUI;component/Themes/ThemeModern.xaml"));
        }

        [TestMethod]
        public void ThemeDictionaries_OverrideSystemBrushesUsedByDefaultTemplates()
        {
            foreach (var themeFile in new[] { "ThemeLight.xaml", "ThemeDark.xaml", "ThemeClassic.xaml" })
            {
                var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", themeFile));

                Assert.Contains("SystemColors.ControlBrushKey", themeXaml);
                Assert.Contains("SystemColors.ControlTextBrushKey", themeXaml);
                Assert.Contains("SystemColors.WindowBrushKey", themeXaml);
                Assert.Contains("SystemColors.WindowTextBrushKey", themeXaml);
                Assert.Contains("SystemColors.HotTrackBrushKey", themeXaml);
            }
        }

        [TestMethod]
        public void ThemeModern_TabControlHeaderUsesDedicatedTabStripBackground()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"))
                .Replace("\r\n", "\n");

            Assert.Contains(
                "<Border x:Name=\"headerBorder\" Grid.Row=\"0\" Grid.Column=\"0\"\n" +
                "                                Background=\"{DynamicResource TabStripBackgroundBrush}\"",
                themeXaml);
            Assert.DoesNotContain( "Background=\"{TemplateBinding Background}\" Padding=\"0\">\n                            <TabPanel", themeXaml );
        }

        [TestMethod]
        public void ThemeModern_TabItemsUseTabStripBackgroundByDefault()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var tabItemStyle = ExtractBetween(
                themeXaml,
                "<Style TargetType=\"TabItem\">",
                "<!-- ============================================ -->" );

            Assert.Contains("Background=\"{DynamicResource TabStripBackgroundBrush}\"", tabItemStyle);
            Assert.DoesNotContain("Background=\"Transparent\"", tabItemStyle);
        }

        [TestMethod]
        public void ThemeModern_TextBoxAndComboBoxUseMatchingPaddingAndHeight()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("<Setter Property=\"Padding\" Value=\"2,2\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"Padding\" Value=\"5,2\"/>", themeXaml);
            Assert.IsGreaterThanOrEqualTo( 2, CountOccurrences(themeXaml, "<Setter Property=\"MinHeight\" Value=\"24\"/>"));
        }

        [TestMethod]
        public void ThemeModern_DataGridShowsVerticalSeparators()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("<Setter Property=\"GridLinesVisibility\" Value=\"All\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"BorderThickness\" Value=\"0,0,1,1\"/>", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_DisabledControlsDoNotUseOpacity()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.DoesNotContain( "<Setter Property=\"Opacity\" Value=\"0.4\"/>", themeXaml );
            Assert.DoesNotContain( "<Setter Property=\"Opacity\" Value=\"0.45\"/>", themeXaml );
            Assert.DoesNotContain( "<Setter Property=\"Opacity\" Value=\"0.5\"/>", themeXaml );
            Assert.Contains("DisabledControlBackgroundBrush", themeXaml);
            Assert.Contains("DisabledControlForegroundBrush", themeXaml);
            Assert.Contains("DisabledControlBorderBrush", themeXaml);
        }

        [TestMethod]
        public void ThemeDictionaries_DefineModernDisabledControlBrushes()
        {
            foreach (var themeFile in new[] { "ThemeLight.xaml", "ThemeDark.xaml" })
            {
                var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", themeFile));

                Assert.Contains("DisabledControlBackgroundBrush", themeXaml);
                Assert.Contains("DisabledControlForegroundBrush", themeXaml);
                Assert.Contains("DisabledControlBorderBrush", themeXaml);
                Assert.Contains("TabStripBackgroundBrush", themeXaml);
            }

            var lightTheme = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeLight.xaml"));
            var darkTheme = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeDark.xaml"));

            Assert.Contains("<Color x:Key=\"DisabledControlBackground\">#FAFAFA</Color>", lightTheme);
             Assert.Contains("<Color x:Key=\"DisabledControlForeground\">#707070</Color>", lightTheme);
             Assert.Contains("<Color x:Key=\"DisabledControlBorder\">#E4E4E4</Color>", lightTheme);
             Assert.Contains("<Color x:Key=\"DisabledControlBackground\">#242424</Color>", darkTheme);
             Assert.Contains("<Color x:Key=\"DisabledControlForeground\">#949494</Color>", darkTheme);
             Assert.Contains("<Color x:Key=\"DisabledControlBorder\">#383838</Color>", darkTheme);
            Assert.Contains("ReadOnlyControlBackgroundBrush", lightTheme);
            Assert.Contains("ReadOnlyControlBackgroundBrush", darkTheme);
        }

        [TestMethod]
        public void ThemeDictionaries_DefineEditableControlBackgrounds()
        {
            foreach (var themeFile in new[] { "ThemeLight.xaml", "ThemeDark.xaml", "ThemeClassic.xaml" })
            {
                var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", themeFile));

                Assert.Contains("EditableControlBackgroundBrush", themeXaml);
            }

            Assert.Contains("<Color x:Key=\"EditableControlBackground\">#FFFFFF</Color>", File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeLight.xaml")));
            Assert.Contains("<Color x:Key=\"EditableControlBackground\">#2D2D2D</Color>", File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeDark.xaml")));
        }

        [TestMethod]
        public void ThemeModern_ComboBoxTemplateCarriesDisabledForegroundIntoButton()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("Foreground=\"{TemplateBinding Foreground}\"", themeXaml);
            Assert.Contains("Fill=\"{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}\"", themeXaml);
            Assert.Contains("<Setter TargetName=\"ToggleButton\" Property=\"Background\" Value=\"{DynamicResource DisabledControlBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Setter TargetName=\"ToggleButton\" Property=\"Foreground\" Value=\"{DynamicResource DisabledControlForegroundBrush}\"/>", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_NonEditableComboBoxUsesFullWidthDropDownToggle()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var comboBoxTemplate = ExtractBetween(
                themeXaml,
                "<ControlTemplate x:Key=\"ComboBoxModernTemplate\" TargetType=\"ComboBox\">",
                "<!-- Modern ComboBox Style -->" );
            var toggleButtonDeclaration = ExtractBetween(
                comboBoxTemplate,
                "<ToggleButton x:Name=\"ToggleButton\"",
                "<ToggleButton.Template>" );

            Assert.Contains("Grid.ColumnSpan=\"2\"", toggleButtonDeclaration);
            Assert.Contains("HorizontalAlignment=\"Stretch\"", toggleButtonDeclaration);
            Assert.Contains("VerticalAlignment=\"Stretch\"", toggleButtonDeclaration);
            Assert.Contains("ClickMode=\"Press\"", toggleButtonDeclaration);
            Assert.Contains("Style=\"{x:Null}\"", toggleButtonDeclaration);
            Assert.DoesNotContain("HorizontalAlignment=\"Right\"", toggleButtonDeclaration);
            var toggleIndex = comboBoxTemplate.IndexOf("<ToggleButton x:Name=\"ToggleButton\"", StringComparison.Ordinal);
            var contentHostIndex = comboBoxTemplate.IndexOf("<Border x:Name=\"ContentHostBorder\"", StringComparison.Ordinal);

            Assert.IsGreaterThanOrEqualTo(0, toggleIndex);
            Assert.IsGreaterThanOrEqualTo(0, contentHostIndex);
            Assert.IsLessThan( contentHostIndex, toggleIndex );
            Assert.Contains("<Border x:Name=\"ContentHostBorder\"", comboBoxTemplate);
            Assert.Contains("IsHitTestVisible=\"False\"", comboBoxTemplate);
            Assert.Contains("MinWidth=\"{x:Static SystemParameters.VerticalScrollBarWidth}\"", comboBoxTemplate);
            Assert.Contains("<Setter TargetName=\"PART_EditableTextBox\" Property=\"IsHitTestVisible\" Value=\"True\"/>", comboBoxTemplate);
            Assert.Contains("<Setter TargetName=\"ContentHostBorder\" Property=\"IsHitTestVisible\" Value=\"True\"/>", comboBoxTemplate);
            Assert.DoesNotContain("LastChildFill=\"True\"", comboBoxTemplate);
            Assert.DoesNotContain("x:Name=\"OuterBorder\"", comboBoxTemplate);
        }

        [TestMethod]
        public void ThemeModern_EditableControlsUseEditableBackground()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.IsGreaterThanOrEqualTo( 3, CountOccurrences(themeXaml, "<Setter Property=\"Background\" Value=\"{DynamicResource EditableControlBackgroundBrush}\"/>"));
            Assert.Contains( "<Setter Property=\"RowBackground\" Value=\"{DynamicResource WindowBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Trigger Property=\"IsReadOnly\" Value=\"True\">", themeXaml);
            Assert.Contains("<Setter Property=\"Background\" Value=\"{DynamicResource ReadOnlyControlBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource ReadOnlyControlForegroundBrush}\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"BorderBrush\" Value=\"{DynamicResource ReadOnlyControlBorderBrush}\"/>", themeXaml);
        }

        [TestMethod]
        public void ThemeDictionaries_DoNotDefineUnusedMarginResources()
        {
            foreach (var themeFile in new[] { "ThemeLight.xaml", "ThemeDark.xaml", "ThemeClassic.xaml" })
            {
                var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", themeFile));

                Assert.DoesNotContain("x:Key=\"Margin", themeXaml);
                Assert.DoesNotContain("x:Key=\"Padding", themeXaml);
            }
        }

        [TestMethod]
        public void ThemeModern_ButtonTemplateCarriesForegroundIntoContent()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("TextElement.Foreground=\"{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}\"", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_ComboBoxDropdownItemsUseThemedTemplate()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("<Style TargetType=\"ComboBoxItem\">", themeXaml);
            Assert.Contains("<ControlTemplate TargetType=\"ComboBoxItem\">", themeXaml);
            Assert.DoesNotContain("SystemColors.HighlightBrushKey", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_ThemesScrollBars()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("<Style TargetType=\"ScrollBar\">", themeXaml);
            Assert.Contains("<Style TargetType=\"Thumb\">", themeXaml);
            Assert.Contains("PART_Track", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_ToolBarTemplateThemesOverflowChrome()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var toolBarStyle = ExtractBetween(
                themeXaml,
                "<Style TargetType=\"ToolBar\">",
                "<Style TargetType=\"ToolBarTray\">" );

            Assert.Contains("<ControlTemplate TargetType=\"ToolBar\">", toolBarStyle);
            Assert.Contains("x:Name=\"OverflowButton\"", toolBarStyle);
            Assert.Contains("ToolBarOverflowPanel", toolBarStyle);
            Assert.Contains("IsOpen=\"{Binding IsOverflowOpen, RelativeSource={RelativeSource TemplatedParent}}\"", toolBarStyle);
            Assert.Contains("Background=\"{DynamicResource ControlBackgroundBrush}\"", toolBarStyle);
            Assert.Contains("BorderBrush=\"{DynamicResource BorderMediumBrush}\"", toolBarStyle);
            Assert.Contains("Fill=\"{DynamicResource TextPrimaryBrush}\"", toolBarStyle);
            Assert.Contains("HasOverflowItems", toolBarStyle);
        }

        [TestMethod]
        public void ThemeModern_DefinesToolBarItemResourceKeyStyles()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var toolBarStyle = ExtractBetween(
                themeXaml,
                "<Style TargetType=\"ToolBar\">",
                "<Style TargetType=\"ToolBarTray\">" );

            Assert.Contains("x:Key=\"{x:Static ToolBar.ButtonStyleKey}\"", toolBarStyle);
            Assert.Contains("x:Key=\"{x:Static ToolBar.ToggleButtonStyleKey}\"", toolBarStyle);
            Assert.Contains("x:Key=\"{x:Static ToolBar.CheckBoxStyleKey}\"", toolBarStyle);
            Assert.Contains("x:Key=\"{x:Static ToolBar.SeparatorStyleKey}\"", toolBarStyle);
            Assert.Contains("<ControlTemplate TargetType=\"{x:Type Button}\">", toolBarStyle);
            Assert.Contains("<ControlTemplate TargetType=\"{x:Type ToggleButton}\">", toolBarStyle);
            Assert.Contains("<ControlTemplate TargetType=\"{x:Type CheckBox}\">", toolBarStyle);
        }

        [TestMethod]
        public void ThemeModern_ToolBarItemStylesUseFlatThemedChrome()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var toolBarStyle = ExtractBetween(
                themeXaml,
                "<Style TargetType=\"ToolBar\">",
                "<Style TargetType=\"ToolBarTray\">" );
            var buttonStyle = ExtractBetween(
                toolBarStyle,
                "<Style x:Key=\"{x:Static ToolBar.ButtonStyleKey}\"",
                "<Style x:Key=\"{x:Static ToolBar.ToggleButtonStyleKey}\"" );
            var toggleStyle = ExtractBetween(
                toolBarStyle,
                "<Style x:Key=\"{x:Static ToolBar.ToggleButtonStyleKey}\"",
                "<Style x:Key=\"{x:Static ToolBar.CheckBoxStyleKey}\"" );
            var checkBoxStyle = ExtractBetween(
                toolBarStyle,
                "<Style x:Key=\"{x:Static ToolBar.CheckBoxStyleKey}\"",
                "<Style x:Key=\"{x:Static ToolBar.RadioButtonStyleKey}\"" );

            Assert.DoesNotContain("BasedOn=", buttonStyle);
            Assert.DoesNotContain("BasedOn=", toggleStyle);
            Assert.DoesNotContain("BasedOn=", checkBoxStyle);
            Assert.Contains("Background\" Value=\"Transparent\"", buttonStyle);
            Assert.Contains("Background\" Value=\"Transparent\"", toggleStyle);
            Assert.Contains("Background\" Value=\"Transparent\"", checkBoxStyle);
            Assert.Contains("x:Name=\"ToolBarButtonBorder\"", buttonStyle);
            Assert.Contains("x:Name=\"ToolBarToggleButtonBorder\"", toggleStyle);
            Assert.Contains("x:Name=\"ToolBarCheckBoxBorder\"", checkBoxStyle);
            Assert.Contains("Property=\"Background\" Value=\"{DynamicResource ControlBackgroundHoverBrush}\"", buttonStyle);
            Assert.Contains("Property=\"Background\" Value=\"{DynamicResource ControlBackgroundHoverBrush}\"", toggleStyle);
            Assert.Contains("Property=\"Background\" Value=\"{DynamicResource ControlBackgroundHoverBrush}\"", checkBoxStyle);
            Assert.Contains("<Trigger Property=\"IsChecked\" Value=\"True\">", toggleStyle);
            Assert.Contains("<Trigger Property=\"IsChecked\" Value=\"True\">", checkBoxStyle);
        }

        [STATestMethod]
        public void ThemeModern_ToolBarTemplateMeasuresWithItems()
        {
            var resourceDictionary = (ResourceDictionary)Application.LoadComponent(
                new Uri("/EddiUI;component/Themes/ThemeModern.xaml", UriKind.Relative));
            var toolBar = new ToolBar
            {
                Style = (Style)resourceDictionary[typeof(ToolBar)]
            };
            toolBar.Items.Add(new Button { Content = "Cut" });
            toolBar.Items.Add(new Button { Content = "Copy" });
            toolBar.Items.Add(new Separator());
            toolBar.Items.Add(new CheckBox { Content = "Line numbers" });
            toolBar.Items.Add(new CheckBox { Content = "Word wrap" });

            var toolBarTray = new ToolBarTray();
            toolBarTray.ToolBars.Add(toolBar);

            toolBarTray.Measure(new Size(120, 40));
            toolBarTray.Arrange(new Rect(0, 0, 120, 40));
            toolBarTray.UpdateLayout();

            Assert.IsGreaterThan( 0, toolBar.DesiredSize.Width);
        }

        [TestMethod]
        public void ThemeClassic_TabsUseGreyButtonsAndKeepGradientOutOfContent()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeClassic.xaml"));

            Assert.Contains("<Setter Property=\"Background\" Value=\"{DynamicResource WindowBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("Background=\"{DynamicResource WindowBackgroundBrush}\"", themeXaml);
            Assert.Contains("Background=\"{DynamicResource ClassicTabHeaderGradientBrush}\"", themeXaml);
            Assert.DoesNotContain( "TargetName=\"headerBorder\" Property=\"Background\" Value=\"{DynamicResource DockPanelBackgroundBrush}\"", themeXaml );
            Assert.Contains("<Setter TargetName=\"contentPanel\" Property=\"Padding\" Value=\"8,0,8,0\"/>", themeXaml);
            Assert.DoesNotContain( "<Setter Property=\"Padding\" Value=\"8,0,0,0\"/>", themeXaml );
            Assert.Contains("<Color x:Key=\"ClassicTabBackground\">#E5E5E5</Color>", themeXaml);
            Assert.Contains("<Setter Property=\"Background\" Value=\"{DynamicResource ClassicTabBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource ClassicTabForegroundBrush}\"/>", themeXaml);
        }

        [TestMethod]
        public void ThemeClassic_EditableTextAndGridsUseWhiteBackground()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeClassic.xaml"));

            Assert.Contains("<Color x:Key=\"EditableControlBackground\">#FFFFFF</Color>", themeXaml);
            Assert.Contains("<Setter Property=\"Background\" Value=\"{DynamicResource EditableControlBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"RowBackground\" Value=\"{DynamicResource EditableControlBackgroundBrush}\"/>", themeXaml);
            Assert.Contains("<Trigger Property=\"IsReadOnly\" Value=\"True\">", themeXaml);
        }

        [TestMethod]
        public void ThemeClassic_TabItemBordersRespectPlacement()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeClassic.xaml"));

            Assert.Contains("<Condition Binding=\"{Binding TabStripPlacement, RelativeSource={RelativeSource AncestorType=TabControl}}\" Value=\"Left\"/>", themeXaml);
            Assert.Contains("<Setter TargetName=\"border\" Property=\"BorderThickness\" Value=\"4,0,0,1\"/>", themeXaml);
            Assert.Contains("<Condition Binding=\"{Binding TabStripPlacement, RelativeSource={RelativeSource AncestorType=TabControl}}\" Value=\"Top\"/>", themeXaml);
            Assert.Contains("<Setter TargetName=\"border\" Property=\"BorderThickness\" Value=\"0,0,1,4\"/>", themeXaml);
            Assert.Contains("<Setter TargetName=\"border\" Property=\"BorderThickness\" Value=\"0,0,1,1\"/>", themeXaml);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource TextPrimaryBrush}\"/>", themeXaml);
        }

        [TestMethod]
        public void ThemeModern_LeftTabContentHasGutter()
        {
            var themeXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));

            Assert.Contains("<Setter TargetName=\"contentPanel\" Property=\"Padding\" Value=\"8,0,8,0\"/>", themeXaml);
        }

        [TestMethod]
        public void MainWindow_VersionLabelUsesTextBlockForBaselineAlignment()
        {
            var mainWindowXaml = File.ReadAllText(FindRepoFile("EddiUI", "MainWindow.xaml"));

            Assert.Contains("<TextBlock x:Name=\"Version\"", mainWindowXaml);
            Assert.DoesNotContain( "<Label Height=\"28\" Margin=\"0,0,0,0\" Name=\"Version\"", mainWindowXaml );
        }

        [TestMethod]
        public void MainWindow_IntroFlowDocumentOwnsItsPagePadding()
        {
            var mainWindowXaml = File.ReadAllText(FindRepoFile("EddiUI", "MainWindow.xaml"));

            Assert.Contains("<FlowDocument PagePadding=\"0\">", mainWindowXaml);
        }

        [TestMethod]
        public void MainWindow_IntroRichTextBoxOwnsItsInternalPadding()
        {
            var mainWindowXaml = File.ReadAllText(FindRepoFile("EddiUI", "MainWindow.xaml"));

            Assert.Contains("RichTextBox DockPanel.Dock=\"Top\" IsReadOnly=\"True\" IsDocumentEnabled=\"True\" BorderThickness=\"0\" Padding=\"0\"", mainWindowXaml);
        }

        [TestMethod]
        public void ScopedTabControls_DoNotCaptureImplicitTabItemStyle()
        {
            foreach (var xamlFile in new[]
            {
                FindRepoFile("EddiUI", "MainWindow.xaml"),
                FindRepoFile("NavigationMonitor", "ConfigurationWindow.xaml")
            })
            {
                var xaml = File.ReadAllText(xamlFile);

                Assert.DoesNotContain("BasedOn=\"{StaticResource {x:Type TabItem}}\"", xaml);
            }
        }

        [TestMethod]
        public void SpeechResponder_GridActionButtonsUseCompactStyle()
        {
            var speechResponderXaml = File.ReadAllText(FindRepoFile("SpeechResponder", "ConfigurationWindow.xaml"));
            var modernXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeModern.xaml"));
            var classicXaml = File.ReadAllText(FindRepoFile("EddiUI", "Themes", "ThemeClassic.xaml"));

            Assert.Contains("x:Key=\"ScriptGridButtonStyle\"", modernXaml);
            Assert.Contains("x:Key=\"ScriptGridButtonStyle\"", classicXaml);
            Assert.Contains("<Setter Property=\"Padding\" Value=\"4,0\" />", modernXaml);
            Assert.Contains("<Setter Property=\"Padding\" Value=\"4,0\" />", classicXaml);
            Assert.Contains("Style=\"{DynamicResource ScriptGridButtonStyle}\"", speechResponderXaml);
            Assert.Contains("Padding=\"4,0\"", speechResponderXaml);
        }

        [TestMethod]
        public void GetFriendlyVoiceName_WebProviderVoice_UsesProviderSuppliedFriendlyName()
        {
            var voice = new VoiceDetails(
                "Salli (Standard)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: "amazon-polly-main",
                providerDisplayName: "Amazon Polly",
                supportedLocales: ["en-US"],
                friendlyName: "English (United States) Salli (Standard) [Amazon Polly]");
            var method = typeof(TextToSpeechTab).GetMethod(
                "GetFriendlyVoiceName",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);
            var friendlyName = (string)method.Invoke(null, [voice]);

            Assert.AreEqual("English (United States) Salli (Standard) [Amazon Polly]", friendlyName);
        }

        private static string FindRepoFile(params string[] pathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException($"Unable to locate {Path.Combine(pathParts)}");
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;
            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string ExtractBetween(string text, string start, string end)
        {
            var startIndex = text.IndexOf(start, StringComparison.Ordinal);
            Assert.IsGreaterThanOrEqualTo(0, startIndex);
            var endIndex = text.IndexOf(end, startIndex, StringComparison.Ordinal);
            Assert.IsGreaterThanOrEqualTo(0, endIndex);

            return text[startIndex..endIndex];
        }

        private static T FindVisualDescendant<T>(DependencyObject root, string name = null)
            where T : FrameworkElement
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T element && (name == null || element.Name == name))
                {
                    return element;
                }

                var descendant = FindVisualDescendant<T>(child, name);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }
    }

    [TestClass, TestCategory( "UnitTests" )]
    public class TabItemHeaderComparerTests : TestBase
    {
        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
        }

        [TestMethod]
        public void CompareHeaderText_SortsAlphabetically ()
        {
            var headers = new List<string> { "Zebra", "Apple", "Banana" };
            var expected = new[] { "Apple", "Banana", "Zebra" };

            headers.Sort( ( x, y ) =>
                MainWindow.TabItemComparer.CompareHeaderText(
                    x,
                    y,
                    StringComparer.CurrentCultureIgnoreCase ) );

            CollectionAssert.AreEqual(
                expected,
                headers );
        }

        [TestMethod]
        public void CompareHeaderText_IgnoresCase ()
        {
            var result = MainWindow.TabItemComparer.CompareHeaderText(
            "APPLE",
            "zebra",
            StringComparer.CurrentCultureIgnoreCase);

            Assert.IsLessThan( 0, result );
        }

        [TestMethod]
        public void CompareHeaderText_HandlesNullHeaders ()
        {
            var result = MainWindow.TabItemComparer.CompareHeaderText(
            null,
            "Valid",
            StringComparer.CurrentCultureIgnoreCase);

            Assert.AreNotEqual( 0, result );
        }
    }

    [STATestClass, TestCategory("WpfTests"), DoNotParallelize]
    public class MainWindowTabItemComparerWpfTests : TestBase
    {
        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
        }

        [TestMethod]
        public void Compare_UsesTabItemHeaderText ()
        {
            var comparer = new MainWindow.TabItemComparer(
            StringComparer.CurrentCultureIgnoreCase);

            var first = new TabItem { Header = "Apple" };
            var second = new TabItem { Header = "Zebra" };

            var result = comparer.Compare(first, second);

            Assert.IsLessThan( 0, result );
        }
    }
}
