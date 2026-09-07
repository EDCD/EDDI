using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Res = EddiMaterialMonitor.Properties.MaterialMonitor;

namespace EddiMaterialMonitor
{
    public partial class ConfigurationWindow : UserControl
    {
        private readonly MaterialMonitor monitor;
        private readonly ListCollectionView view;
        private readonly DispatcherTimer saveTimer;
        private readonly HashSet<MaterialAmount> observed = new();
        private bool initializing = true;
        private bool subscribed;
        private bool refreshQueued;
        private string namePath = nameof(MaterialAmount.material);
        private string categoryPath = nameof(MaterialAmount.category);
        private string resourceSignature;
        private string choiceSignature;

        internal sealed class Choice<T>
        {
            public T Value { get; set; }
            public string Label { get; set; }
        }

        public ConfigurationWindow() : this(
            (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor"),
            ConfigService.Instance.materialMonitorConfiguration) { }

        internal ConfigurationWindow(MaterialMonitor monitor, MaterialMonitorConfiguration config)
        {
            this.monitor = monitor;
            InitializeComponent();
            saveTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(300), DispatcherPriority.Background,
                (_, _) => SavePreferences(), Dispatcher);
            saveTimer.Stop();
            if (monitor == null) { return; }

            view = new ListCollectionView( monitor.inventory )
            {
                Filter = item => MaterialViewFilter.Matches( (MaterialAmount)item, searchText.Text,
                    ( typeFilter.SelectedItem as Choice<string> )?.Value,
                    ( gradeFilter.SelectedItem as Choice<int?> )?.Value,
                    ( inventoryFilter.SelectedItem as Choice<string> )?.Value )
            };
            invariantNames.IsChecked = config?.useInvariantNames == true;
            searchText.Text = config?.searchText ?? "";
            PopulateChoices(config?.categoryFilter, config?.gradeFilter, config?.inventoryFilter);
            materialsData.ItemsSource = view;
            initializing = false;
            ApplyView();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            materialsData.RowEditEnding += (_, _) => QueueRefresh();
        }

        private void PopulateChoices(string category, int? grade, string status)
        {
            var wasInitializing = initializing;
            initializing = true;
            var definitions = MaterialViewFilter.Definitions();
            var (unknownCategory, unknownRarity) = AvailableUnknownFilters();
            var types = new List<Choice<string>> { new() { Label = Res.filter_all } };
            types.AddRange(definitions.Select(m => m.Category)
                .Where(c => c != null && (c != MaterialCategory.Unknown || unknownCategory)).Distinct()
                .Select(c => new Choice<string> { Value = c.edname, Label = invariantNames.IsChecked == true ? c.invariantName : c.localizedName })
                .OrderBy(c => c.Label));
            typeFilter.ItemsSource = types;
            typeFilter.SelectedItem = types.FirstOrDefault(c => c.Value == category) ?? types[0];
            var grades = new List<Choice<int?>> { new() { Label = Res.filter_all } };
            grades.AddRange(definitions.Select(m => m.Rarity?.level ?? 0)
                .Where(g => g != 0 || unknownRarity).Distinct().OrderBy(g => g)
                .Select(g => new Choice<int?> { Value = g, Label = g == 0 ? Res.filter_unknown : g.ToString(CultureInfo.CurrentCulture) }));
            gradeFilter.ItemsSource = grades;
            gradeFilter.SelectedItem = grades.FirstOrDefault(g => g.Value == grade) ?? grades[0];
            var statuses = new[]
            {
                new Choice<string> { Value = "All", Label = Res.filter_all },
                new Choice<string> { Value = "Owned", Label = Res.filter_owned },
                new Choice<string> { Value = "BelowMinimum", Label = Res.filter_below_minimum },
                new Choice<string> { Value = "BelowDesired", Label = Res.filter_below_desired }
            };
            inventoryFilter.ItemsSource = statuses;
            inventoryFilter.SelectedItem = statuses.FirstOrDefault(s => s.Value == status) ?? statuses[0];
            initializing = wasInitializing;
        }

        private (bool Category, bool Rarity) AvailableUnknownFilters()
        {
            // Use inventory eligible for the grid before user-selected filters, so a
            // selection cannot remove itself merely by filtering out another category.
            var items = monitor.InventorySnapshot()
                .Where(item => MaterialViewFilter.Matches(item, null, null, null, "All")).ToList();
            return (items.Any(item => item.MaterialDef?.Category == MaterialCategory.Unknown),
                items.Any(item => item.Rarity == Rarity.Unknown));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (subscribed) { return; }
            subscribed = true;
            monitor.InventoryUpdatedEvent += InventoryUpdated;
            monitor.inventory.CollectionChanged += CollectionChanged;
            ((INotifyPropertyChanged)view).PropertyChanged += ViewChanged;
            ObserveItems();
            ApplyView();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SavePreferences();
            subscribed = false;
            monitor.InventoryUpdatedEvent -= InventoryUpdated;
            monitor.inventory.CollectionChanged -= CollectionChanged;
            ((INotifyPropertyChanged)view).PropertyChanged -= ViewChanged;
            foreach (var item in observed) { item.PropertyChanged -= ItemChanged; }
            observed.Clear();
        }

        private void ObserveItems()
        {
            var current = monitor.InventorySnapshot().ToHashSet();
            foreach (var item in observed.Except(current).ToList())
            {
                item.PropertyChanged -= ItemChanged;
                observed.Remove(item);
            }
            foreach (var item in current.Except(observed).ToList())
            {
                item.PropertyChanged += ItemChanged;
                observed.Add(item);
            }
        }

        private void InventoryUpdated(object sender, EventArgs e) => QueueRefresh();
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => QueueRefresh();
        private void ItemChanged(object sender, PropertyChangedEventArgs e) => QueueRefresh();
        private void ViewChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(view.IsEditingItem) && !view.IsEditingItem) { QueueRefresh(); }
        }

        private void QueueRefresh()
        {
            if (!Dispatcher.CheckAccess()) { Dispatcher.BeginInvoke(new Action(QueueRefresh)); return; }
            if (refreshQueued || !subscribed) { return; }
            refreshQueued = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                refreshQueued = false;
                if (!subscribed) { return; }
                ObserveItems();
                ApplyView();
            }));
        }

        private void ApplyView()
        {
            if (view == null || view.IsEditingItem || view.IsAddingNew) { return; }
            var category = (typeFilter.SelectedItem as Choice<string>)?.Value;
            var grade = (gradeFilter.SelectedItem as Choice<int?>)?.Value;
            var status = (inventoryFilter.SelectedItem as Choice<string>)?.Value;
            var definitions = MaterialViewFilter.Definitions();
            var signature = CultureInfo.CurrentUICulture.Name + "|" + string.Join("|", definitions.Select(m => m.edname));
            var nextChoices = signature + "|" + invariantNames.IsChecked + "|" + AvailableUnknownFilters();
            if (choiceSignature != nextChoices)
            {
                PopulateChoices(category, grade, status);
                choiceSignature = nextChoices;
                if (category != (typeFilter.SelectedItem as Choice<string>)?.Value ||
                    grade != (gradeFilter.SelectedItem as Choice<int?>)?.Value)
                {
                    SavePreferences();
                }
            }
            if (resourceSignature != signature)
            {
                invariantNames.Visibility = MaterialViewFilter.HasLocalizedNames(CultureInfo.CurrentUICulture, definitions)
                    ? Visibility.Visible : Visibility.Collapsed;
                resourceSignature = signature;
            }
            var nextName = invariantNames.IsChecked == true ? "MaterialDef.invariantName" : "material";
            var nextCategory = invariantNames.IsChecked == true ? "MaterialDef.Category.invariantName" : "category";
            using (view.DeferRefresh())
            {
                for (var i = 0; i < view.SortDescriptions.Count; i++)
                {
                    var sort = view.SortDescriptions[i];
                    var path = sort.PropertyName == namePath ? nextName : sort.PropertyName == categoryPath ? nextCategory : sort.PropertyName;
                    if (path != sort.PropertyName) { view.SortDescriptions[i] = new SortDescription(path, sort.Direction); }
                }
                namePath = nextName;
                categoryPath = nextCategory;
                if ((nameColumn.Binding as Binding)?.Path?.Path != namePath) { nameColumn.Binding = new Binding(namePath); }
                nameColumn.SortMemberPath = namePath;
                if ((categoryColumn.Binding as Binding)?.Path?.Path != categoryPath) { categoryColumn.Binding = new Binding(categoryPath); }
                categoryColumn.SortMemberPath = categoryPath;
            }
            view.Refresh();
        }

        private void controlsChanged(object sender, RoutedEventArgs e)
        {
            if (initializing) { return; }
            ApplyView();
            SavePreferences();
        }

        private void searchChanged(object sender, TextChangedEventArgs e)
        {
            if (initializing) { return; }
            ApplyView();
            saveTimer.Stop();
            saveTimer.Start();
        }

        private void clearFilters(object sender, RoutedEventArgs e)
        {
            initializing = true;
            searchText.Text = "";
            typeFilter.SelectedIndex = gradeFilter.SelectedIndex = inventoryFilter.SelectedIndex = 0;
            initializing = false;
            ApplyView();
            SavePreferences();
        }

        private void SavePreferences()
        {
            saveTimer.Stop();
            if (initializing) { return; }
            MaterialMonitor.SaveViewPreferences(invariantNames.IsChecked == true, searchText.Text,
                (typeFilter.SelectedItem as Choice<string>)?.Value,
                (gradeFilter.SelectedItem as Choice<int?>)?.Value,
                (inventoryFilter.SelectedItem as Choice<string>)?.Value ?? "All");
        }

        private void materialsUpdated(object sender, DataTransferEventArgs e)
        {
            if (!initializing) { monitor?.writeMaterials(); }
        }
    }
}
