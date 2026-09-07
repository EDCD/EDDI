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
using System.Windows.Input;
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
        private MaterialLevelPreview bulkPreview;
        private IReadOnlyList<MaterialLevelChange> undoChanges;

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
            InitializeBulkEditor();
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
            undoChanges = null;
            undoBulkLevels.IsEnabled = false;
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
            UpdateBulkPreview();
            ValidateUndoAvailability();
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

        private void InitializeBulkEditor()
        {
            var actions = new[]
            {
                new Choice<MaterialLevelAction> { Value = MaterialLevelAction.LeaveUnchanged, Label = Res.leave_unchanged },
                new Choice<MaterialLevelAction> { Value = MaterialLevelAction.FillBlanks, Label = Res.fill_blanks },
                new Choice<MaterialLevelAction> { Value = MaterialLevelAction.ReplaceValues, Label = Res.replace_values },
                new Choice<MaterialLevelAction> { Value = MaterialLevelAction.ClearThreshold, Label = Res.clear_threshold }
            };
            minimumAction.ItemsSource = actions;
            desiredAction.ItemsSource = actions;
            minimumAction.SelectedIndex = desiredAction.SelectedIndex = 0;
        }

        private void bulkControlsChanged(object sender, RoutedEventArgs e)
        {
            if (initializing) { return; }
            bulkStatus.Text = "";
            UpdateBulkPreview();
        }

        private void bulkValuePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !char.IsDigit(c));
        }

        private MaterialLevelAction SelectedAction(ComboBox box) =>
            (box.SelectedItem as Choice<MaterialLevelAction>)?.Value ?? MaterialLevelAction.LeaveUnchanged;

        private bool TryReadBulkValue(TextBox box, MaterialLevelAction action, out int? value, out string error)
        {
            value = null;
            error = null;
            var requiresValue = action is MaterialLevelAction.FillBlanks or MaterialLevelAction.ReplaceValues;
            box.Visibility = requiresValue ? Visibility.Visible : Visibility.Collapsed;
            if (!requiresValue) { return true; }
            var maximum = percentageUnits.IsChecked == true ? 100 : int.MaxValue;
            if (!int.TryParse(box.Text, NumberStyles.None, CultureInfo.CurrentCulture, out var parsed) ||
                parsed < 0 || parsed > maximum)
            {
                error = string.Format(CultureInfo.CurrentCulture, Res.bulk_invalid_number, 0,
                    percentageUnits.IsChecked == true ? "100" : Res.max_header.ToLower(CultureInfo.CurrentCulture));
                return false;
            }
            value = parsed;
            return true;
        }

        private List<MaterialAmount> BulkTargets()
        {
            return allTargets.IsChecked == true
                ? monitor.InventorySnapshot().Where(MaterialViewFilter.IsEligible).ToList()
                : view?.Cast<MaterialAmount>().ToList() ?? [];
        }

        private MaterialLevelPreview CreateBulkPreview(out string inputError)
        {
            var minimum = SelectedAction(minimumAction);
            var desired = SelectedAction(desiredAction);
            unitsPanel.Visibility = minimum is MaterialLevelAction.FillBlanks or MaterialLevelAction.ReplaceValues ||
                                    desired is MaterialLevelAction.FillBlanks or MaterialLevelAction.ReplaceValues
                ? Visibility.Visible : Visibility.Collapsed;
            var minimumValid = TryReadBulkValue(minimumBulkValue, minimum, out var minimumValue, out var minimumError);
            var desiredValid = TryReadBulkValue(desiredBulkValue, desired, out var desiredValue, out var desiredError);
            inputError = minimumError ?? desiredError;
            if (!minimumValid || !desiredValid) { return null; }
            return MaterialLevelEditor.Preview(BulkTargets(), minimum, minimumValue, desired, desiredValue,
                percentageUnits.IsChecked == true ? MaterialLevelUnits.Percentage : MaterialLevelUnits.Quantity);
        }

        private void UpdateBulkPreview()
        {
            if (monitor == null || minimumAction == null) { return; }
            bulkPreview = CreateBulkPreview(out var inputError);
            if (bulkPreview == null)
            {
                bulkSummary.Text = inputError;
                bulkConflicts.ItemsSource = Array.Empty<string>();
                bulkDetails.ItemsSource = Array.Empty<string>();
                applyBulkLevels.IsEnabled = false;
                return;
            }

            bulkSummary.Text = string.Format(CultureInfo.CurrentCulture, Res.bulk_summary,
                bulkPreview.TargetCount, bulkPreview.Changes.Count,
                bulkPreview.TargetCount - bulkPreview.Changes.Count - bulkPreview.Conflicts.Count);
            bulkConflicts.ItemsSource = bulkPreview.Conflicts.Select(conflict => string.Format(
                CultureInfo.CurrentCulture, Res.bulk_conflict, DisplayName(conflict.EDName),
                ValidationMessage(conflict.Error))).ToList();
            bulkDetails.ItemsSource = bulkPreview.Changes.Select(change => string.Format(CultureInfo.CurrentCulture,
                Res.bulk_change_detail, DisplayName(change.EDName), DisplayValue(change.PreviousMinimum),
                DisplayValue(change.NewMinimum), DisplayValue(change.PreviousDesired), DisplayValue(change.NewDesired))).ToList();
            bulkDetailsExpander.Visibility = bulkPreview.Changes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            applyBulkLevels.IsEnabled = bulkPreview.Changes.Count > 0 && bulkPreview.Conflicts.Count == 0;
        }

        private string DisplayName(string edname)
        {
            var definition = Material.FromEDName(edname);
            return invariantNames.IsChecked == true ? definition?.invariantName ?? edname : definition?.localizedName ?? edname;
        }

        private static string DisplayValue(int? value) => value?.ToString(CultureInfo.CurrentCulture) ?? Res.blank_value;

        private static string ValidationMessage(MaterialLevelValidationError error) => error switch
        {
            MaterialLevelValidationError.Negative => Res.bulk_negative,
            MaterialLevelValidationError.ExceedsCapacity => Res.bulk_exceeds_capacity,
            MaterialLevelValidationError.MinimumExceedsDesired => Res.bulk_minimum_exceeds_desired,
            _ => ""
        };

        private void applyBulkLevelsClick(object sender, RoutedEventArgs e)
        {
            materialsData.CommitEdit(DataGridEditingUnit.Cell, true);
            materialsData.CommitEdit(DataGridEditingUnit.Row, true);
            view.Refresh();
            var displayedSignature = bulkPreview?.Signature;
            var currentPreview = CreateBulkPreview(out var error);
            if (currentPreview == null)
            {
                bulkStatus.Text = error;
                UpdateBulkPreview();
                return;
            }
            if (displayedSignature != currentPreview.Signature)
            {
                bulkStatus.Text = Res.bulk_stale;
                UpdateBulkPreview();
                bulkStatus.Text = Res.bulk_stale;
                return;
            }
            if (currentPreview.Conflicts.Count > 0 || currentPreview.Changes.Count == 0) { UpdateBulkPreview(); return; }
            if (!monitor.TryApplyMaterialLevelChanges(currentPreview.Changes))
            {
                bulkStatus.Text = Res.bulk_stale;
                UpdateBulkPreview();
                bulkStatus.Text = Res.bulk_stale;
                return;
            }
            undoChanges = currentPreview.Changes;
            undoBulkLevels.IsEnabled = true;
            bulkStatus.Text = string.Format(CultureInfo.CurrentCulture, Res.bulk_applied, undoChanges.Count);
            ApplyView();
        }

        private void undoBulkLevelsClick(object sender, RoutedEventArgs e)
        {
            if (undoChanges == null || !monitor.TryApplyMaterialLevelChanges(undoChanges, true))
            {
                undoChanges = null;
                undoBulkLevels.IsEnabled = false;
                bulkStatus.Text = Res.bulk_undo_unavailable;
                return;
            }
            var count = undoChanges.Count;
            undoChanges = null;
            undoBulkLevels.IsEnabled = false;
            bulkStatus.Text = string.Format(CultureInfo.CurrentCulture, Res.bulk_undone, count);
            ApplyView();
        }

        private void ValidateUndoAvailability()
        {
            if (undoChanges == null) { return; }
            var current = monitor.InventorySnapshot().ToDictionary(item => item.edname);
            if (undoChanges.Any(change => !current.TryGetValue(change.EDName, out var item) ||
                    item.minimum != change.NewMinimum || item.desired != change.NewDesired ||
                    !MaterialLevelEditor.IsValid(change.PreviousMinimum, change.PreviousDesired, item.maximum, out _)))
            {
                undoChanges = null;
                undoBulkLevels.IsEnabled = false;
                bulkStatus.Text = Res.bulk_undo_unavailable;
            }
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
