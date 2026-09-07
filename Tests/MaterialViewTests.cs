using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiMaterialMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Tests
{
    [STATestClass, TestCategory("UnitTests"), DoNotParallelize]
    public class MaterialViewTests : TestBase
    {
        public TestContext TestContext { get; set; }
        [TestInitialize]
        public void Initialize() => MakeSafe();

        [TestMethod]
        public void DefinitionSurvivesConstructionReassignmentAndJson()
        {
            var item = new MaterialAmount(Material.Iron, 3, 5, 10);
            Assert.AreSame(Material.Iron, item.MaterialDef);
            var notifications = new System.Collections.Generic.List<string>();
            item.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
            item.material = Material.Carbon.invariantName;
            Assert.AreSame(Material.Carbon, item.MaterialDef);
            Assert.AreEqual(Material.Carbon.edname, item.edname);
            Assert.AreEqual(Material.Carbon.Category.localizedName, item.category);
            Assert.AreSame(Material.Carbon.Rarity, item.Rarity);
            CollectionAssert.Contains(notifications, nameof(MaterialAmount.MaterialDef));
            var json = JsonConvert.SerializeObject(item);
            Assert.DoesNotContain( "MaterialDef", json );
            Assert.AreSame(Material.Carbon, JsonConvert.DeserializeObject<MaterialAmount>(json).MaterialDef);
            var legacy = JsonConvert.DeserializeObject<MaterialAmount>("{\"material\":\"Iron\",\"amount\":2}");
            Assert.AreSame(Material.Iron, legacy.MaterialDef);
            Assert.AreSame(Material.Iron, new MaterialAmount("iron", 1, null, null).MaterialDef);
        }

        [TestMethod]
        public void FiltersCombineAndRespectThresholdBoundaries()
        {
            var item = new MaterialAmount(Material.Iron, 2, 3, 4);
            Assert.IsTrue(MaterialViewFilter.Matches(item, " IRON ", "Element", 1, "BelowMinimum"));
            Assert.IsFalse(MaterialViewFilter.Matches(item, "iron", "Manufactured", 1, "All"));
            Assert.IsFalse(MaterialViewFilter.Matches(item, "iron", "Element", 2, "All"));
            Assert.IsFalse(MaterialViewFilter.Matches(item, "other", null, null, "All"));
            Assert.IsTrue(MaterialViewFilter.Matches(item, "  ", null, null, "Owned"));
            item.amount = 3;
            Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, "BelowMinimum"));
            Assert.IsTrue(MaterialViewFilter.Matches(item, null, null, null, "BelowDesired"));
            item.amount = 4;
            Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, "BelowDesired"));
            item.amount = 0;
            item.minimum = item.desired = null;
            Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, "Owned"));
            Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, "BelowMinimum"));
            Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, "BelowDesired"));
            var unknown = new MaterialAmount("test_unknown_material", 0, null, null);
            Assert.IsFalse(MaterialViewFilter.Matches(unknown, "unknown", "Unknown", 0, "All"));
            unknown.amount = 1;
            Assert.IsTrue(MaterialViewFilter.Matches(unknown, "unknown", "Unknown", 0, "All"));
        }

        [TestMethod]
        public void EmptyMaterialsWithEitherUnknownCategoryOrRarityAreExcluded()
        {
            var unknownCategory = new MaterialAmount(Material.SearchRescueVoucher, 0, 1, 2)
                { Rarity = Rarity.Common };
            var unknownRarity = new MaterialAmount(Material.Iron, 0, 1, 2)
                { Rarity = Rarity.Unknown };
            foreach (var item in new[] { unknownCategory, unknownRarity })
            {
                foreach (var status in new[] { "All", "BelowMinimum", "BelowDesired" })
                {
                    Assert.IsFalse(MaterialViewFilter.Matches(item, null, null, null, status));
                }
                item.amount = 1;
                Assert.IsTrue(MaterialViewFilter.Matches(item, null, null, null, "All"));
            }
            Assert.IsTrue(MaterialViewFilter.Matches(new MaterialAmount(Material.Iron, 0), null, null, null, "All"));
        }

        [TestMethod]
        public void SearchAcceptsLocalizedInvariantAndInternalNames()
        {
            var original = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
                var item = new MaterialAmount(Material.Iron, 1);
                Assert.AreNotEqual(item.material, item.MaterialDef.invariantName);
                foreach (var query in new[] { item.material, item.MaterialDef.invariantName, item.edname })
                {
                    Assert.IsTrue(MaterialViewFilter.Matches(item, query, null, null, "All"));
                }
            }
            finally { CultureInfo.CurrentUICulture = original; }
        }

        [TestMethod]
        public void EdnameAssignmentResolvesDefinitionAndNameUsesCurrentCulture()
        {
            var original = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                var item = new MaterialAmount(Material.Carbon, 1);
                var notifications = new System.Collections.Generic.List<string>();
                item.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
                item.edname = Material.Iron.edname;
                Assert.AreSame(Material.Iron, item.MaterialDef);
                Assert.AreEqual(Material.Iron.invariantName, item.material);
                Assert.AreEqual(Material.Iron.Category.localizedName, item.category);
                Assert.AreSame(Material.Iron.Rarity, item.Rarity);
                CollectionAssert.Contains(notifications, nameof(MaterialAmount.MaterialDef));
                CollectionAssert.Contains(notifications, nameof(MaterialAmount.edname));
                CollectionAssert.Contains(notifications, nameof(MaterialAmount.material));

                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
                Assert.AreEqual(Material.Iron.localizedName, item.material);
                Assert.AreNotEqual(Material.Iron.invariantName, item.material);

                JsonConvert.PopulateObject("{\"edname\":\"carbon\"}", item);
                Assert.AreSame(Material.Carbon, item.MaterialDef);
                Assert.AreEqual(Material.Carbon.localizedName, item.material);
                item.edname = null;
                Assert.IsNull(item.MaterialDef);
                Assert.IsNull(item.material);
            }
            finally { CultureInfo.CurrentUICulture = original; }
        }

        [TestMethod]
        public void ActualResourcesDistinguishFallbackFromTranslation()
        {
            foreach (var culture in new[] { "", "en", "en-US", "en-GB", "zu-ZA" })
            {
                Assert.IsFalse(MaterialViewFilter.HasLocalizedNames(CultureInfo.GetCultureInfo(culture), new[] { Material.Iron }), culture);
            }
            Assert.IsTrue(MaterialViewFilter.HasLocalizedNames(CultureInfo.GetCultureInfo("de-DE"), new[] { Material.Iron }));
        }

        private sealed class TestResources : ResourceSet
        {
            private readonly string[] keys;
            internal TestResources(params string[] keys)
            {
                this.keys = keys;
            }
            public override string GetString(string name) => keys.Contains(name) ? "same as invariant" : null;
        }

        [TestMethod]
        public void ProvenanceSupportsRegionalPartialAndParentResources()
        {
            using var resources = new TestResources("iron");
            bool Has(string culture, string resourceCulture, params string[] keys) =>
                MaterialViewFilter.HasLocalizedResources(CultureInfo.GetCultureInfo(culture), "en", keys,
                    c => c.Name == resourceCulture ? resources : null);
            Assert.IsTrue(Has("en-GB", "en-GB", "iron"));
            Assert.IsTrue(Has("de-DE", "de", "missing", "iron"));
            Assert.IsFalse(Has("de-DE", "de", "missing"));
            Assert.IsFalse(Has("en-US", "en", "iron"));
            Assert.IsFalse(Has("", "", "iron"));
        }

        [TestMethod]
        public void PreferencesRoundTripAndSurviveInventoryWrites()
        {
            var previous = ConfigService.Instance.materialMonitorConfiguration;
            try
            {
                var config = JsonConvert.DeserializeObject<MaterialMonitorConfiguration>("{\"materials\":[]}");
                Assert.IsFalse(config.useInvariantNames);
                Assert.AreEqual("", config.searchText);
                Assert.IsNull(config.gradeFilter);
                Assert.AreEqual("All", config.inventoryFilter);
                ConfigService.Instance.materialMonitorConfiguration = config;
                var monitor = new MaterialMonitor();
                MaterialMonitor.SaveViewPreferences( true, "iron", "Element", 1, "Owned");
                monitor.writeMaterials();
                var restored = JsonConvert.DeserializeObject<MaterialMonitorConfiguration>(
                    JsonConvert.SerializeObject(ConfigService.Instance.materialMonitorConfiguration));
                Assert.IsTrue(restored.useInvariantNames);
                Assert.AreEqual("iron", restored.searchText);
                Assert.AreEqual("Element", restored.categoryFilter);
                Assert.AreEqual(1, restored.gradeFilter);
                Assert.AreEqual("Owned", restored.inventoryFilter);
            }
            finally { ConfigService.Instance.materialMonitorConfiguration = previous; }
        }

        [TestMethod]
        public void UnknownFilterChoicesFollowEligibleInventory()
        {
            var previous = ConfigService.Instance.materialMonitorConfiguration;
            var monitor = new MaterialMonitor();
            monitor.inventory.Clear();
            var unknownCategory = new MaterialAmount(Material.SearchRescueVoucher, 0) { Rarity = Rarity.Common };
            var unknownRarity = new MaterialAmount(Material.Iron, 0) { Rarity = Rarity.Unknown };
            monitor.inventory.Add(unknownCategory);
            monitor.inventory.Add(unknownRarity);
            var panel = new EddiMaterialMonitor.ConfigurationWindow(monitor, new MaterialMonitorConfiguration());
            var types = (ComboBox)panel.FindName("typeFilter");
            var grades = (ComboBox)panel.FindName("gradeFilter");
            bool HasUnknownCategory() => types.Items.Cast<EddiMaterialMonitor.ConfigurationWindow.Choice<string>>()
                .Any(c => c.Value == "Unknown");
            bool HasUnknownRarity() => grades.Items.Cast<EddiMaterialMonitor.ConfigurationWindow.Choice<int?>>()
                .Any(c => c.Value == 0);
            panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            try
            {
                Assert.IsFalse(HasUnknownCategory());
                Assert.IsFalse(HasUnknownRarity());
                unknownCategory.amount = 1;
                PumpDispatcher();
                Assert.IsTrue(HasUnknownCategory());
                Assert.IsFalse(HasUnknownRarity());
                unknownRarity.amount = 1;
                PumpDispatcher();
                Assert.IsTrue(HasUnknownRarity());
                types.SelectedItem = types.Items.Cast<EddiMaterialMonitor.ConfigurationWindow.Choice<string>>()
                    .Single(c => c.Value == "Unknown");
                Assert.IsTrue(HasUnknownCategory());
                Assert.IsTrue(HasUnknownRarity());
                unknownCategory.amount = 0;
                PumpDispatcher();
                Assert.IsFalse(HasUnknownCategory());
                Assert.AreEqual(0, types.SelectedIndex);
                Assert.IsNull(ConfigService.Instance.materialMonitorConfiguration.categoryFilter);
                grades.SelectedItem = grades.Items.Cast<EddiMaterialMonitor.ConfigurationWindow.Choice<int?>>()
                    .Single(c => c.Value == 0);
                monitor.inventory.Remove(unknownRarity);
                PumpDispatcher();
                Assert.IsFalse(HasUnknownRarity());
                Assert.AreEqual(0, grades.SelectedIndex);
                Assert.IsNull(ConfigService.Instance.materialMonitorConfiguration.gradeFilter);
            }
            finally
            {
                panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                ConfigService.Instance.materialMonitorConfiguration = previous;
            }
        }

        private static void PumpDispatcher()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }

        [TestMethod]
        public void PanelSwitchesBothColumnsAndPreservesSortingAndPreferences()
        {
            var original = CultureInfo.CurrentUICulture;
            var previous = ConfigService.Instance.materialMonitorConfiguration;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
                var monitor = new MaterialMonitor();
                var panel = new EddiMaterialMonitor.ConfigurationWindow(monitor, new MaterialMonitorConfiguration());
                var grid = (DataGrid)panel.FindName("materialsData");
                var toggle = (CheckBox)panel.FindName("invariantNames");
                var view = (ListCollectionView)grid.ItemsSource;
                view.SortDescriptions.Add(new SortDescription("material", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("category", ListSortDirection.Ascending));
                toggle.IsChecked = true;
                Assert.AreEqual("MaterialDef.invariantName", grid.Columns[0].SortMemberPath);
                Assert.AreEqual("MaterialDef.Category.invariantName", grid.Columns[1].SortMemberPath);
                Assert.AreEqual(ListSortDirection.Descending, view.SortDescriptions[0].Direction);
                Assert.AreEqual("MaterialDef.Category.invariantName", view.SortDescriptions[1].PropertyName);
                Assert.AreEqual(Visibility.Visible, toggle.Visibility);
                ((TextBox)panel.FindName("searchText")).Text = "no matching material";
                Assert.IsTrue(view.IsEmpty);
                Assert.AreEqual(Visibility.Visible, toggle.Visibility);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                ((TextBox)panel.FindName("searchText")).Text = "iron";
                Assert.AreEqual(Visibility.Collapsed, toggle.Visibility);
                Assert.IsTrue(toggle.IsChecked);
                panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                Assert.IsTrue(ConfigService.Instance.materialMonitorConfiguration.useInvariantNames);
                Assert.AreEqual("iron", ConfigService.Instance.materialMonitorConfiguration.searchText);
            }
            finally
            {
                CultureInfo.CurrentUICulture = original;
                ConfigService.Instance.materialMonitorConfiguration = previous;
            }
        }

        [TestMethod]
        public void PanelNormalizesInvalidSelections()
        {
            var panel = new EddiMaterialMonitor.ConfigurationWindow(new MaterialMonitor(), new MaterialMonitorConfiguration
            { categoryFilter = "invalid", gradeFilter = 100, inventoryFilter = "invalid" });
            foreach (var name in new[] { "typeFilter", "gradeFilter", "inventoryFilter" })
            {
                Assert.AreEqual(0, ((ComboBox)panel.FindName(name)).SelectedIndex);
            }
        }

        [TestMethod]
        public void PanelRefreshesLiveInventoryAndDefersChangesDuringEdits()
        {
            var previous = ConfigService.Instance.materialMonitorConfiguration;
            var monitor = new MaterialMonitor();
            monitor.inventory.Clear();
            var iron = new MaterialAmount(Material.Iron, 0, 2, 5);
            monitor.inventory.Add(iron);
            var panel = new EddiMaterialMonitor.ConfigurationWindow(monitor,
                new MaterialMonitorConfiguration { inventoryFilter = "Owned" });
            var view = (ListCollectionView)((DataGrid)panel.FindName("materialsData")).ItemsSource;
            panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            try
            {
                Assert.IsTrue(view.IsEmpty);
                iron.amount = 1;
                PumpDispatcher();
                Assert.AreEqual(1, view.Count);
                var carbon = new MaterialAmount(Material.Carbon, 2);
                monitor.inventory.Add(carbon);
                PumpDispatcher();
                Assert.AreEqual(2, view.Count);
                monitor.inventory.Remove(carbon);
                PumpDispatcher();
                Assert.AreEqual(1, view.Count);
                view.EditItem(iron);
                ((TextBox)panel.FindName("searchText")).Text = "carbon";
                Assert.AreEqual(1, view.Count);
                view.CommitEdit();
                PumpDispatcher();
                Assert.IsTrue(view.IsEmpty);
                iron.amount = 2;
                ((TextBox)panel.FindName("searchText")).Text = "";
                ((ComboBox)panel.FindName("inventoryFilter")).SelectedIndex = 2;
                Assert.IsTrue(view.IsEmpty);
                iron.minimum = 3;
                PumpDispatcher();
                Assert.AreEqual(1, view.Count);
            }
            finally
            {
                panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                ConfigService.Instance.materialMonitorConfiguration = previous;
            }
        }

        [TestMethod]
        public void PanelLayoutWrapsControlsAtNarrowWidths()
        {
            var original = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
                var panel = new ConfigurationWindow( new MaterialMonitor(), new MaterialMonitorConfiguration() )
                {
                    Background = System.Windows.Media.Brushes.White
                };
                foreach (var width in new[] { 800, 480 })
                {
                    var size = new Size(width, 600);
                    panel.Measure(size);
                    panel.Arrange(new Rect(size));
                    panel.UpdateLayout();
                    PumpDispatcher();
                    panel.UpdateLayout();
                    var search = (TextBox)panel.FindName("searchText");
                    var status = (ComboBox)panel.FindName("inventoryFilter");
                    var searchPosition = search.TransformToAncestor(panel).Transform(new Point());
                    var statusPosition = status.TransformToAncestor(panel).Transform(new Point());
                    Assert.IsLessThanOrEqualTo( width, statusPosition.X + status.ActualWidth);
                    if (width == 480) { Assert.IsGreaterThan( searchPosition.Y, statusPosition.Y); }
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(width, 600, 96, 96,
                        System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(panel);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    var directory = System.IO.Path.Combine(AppContext.BaseDirectory, "MaterialMonitorLayout");
                    System.IO.Directory.CreateDirectory(directory);
                    var path = System.IO.Path.Combine(directory, $"material-monitor-{width}.png");
                    using (var stream = System.IO.File.Create(path)) { encoder.Save(stream); }
                    TestContext.AddResultFile(path);
                }
            }
            finally { CultureInfo.CurrentUICulture = original; }
        }
    }
}
