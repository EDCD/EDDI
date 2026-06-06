using EddiCore;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace EddiCrimeMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow
    {
        private static CrimeMonitor crimeMonitor()
        {
            return (CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            var monitor = crimeMonitor();
            if ( monitor != null )
            {
                criminalRecord.ItemsSource = monitor.criminalrecord;
                monitor.RecordUpdatedEvent += crimeMonitorUpdated;
                Unloaded += ConfigurationWindow_Unloaded;
            }
            updateModifiersEstimateSummary();
        }

        private void ConfigurationWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            var monitor = crimeMonitor();
            if ( monitor != null )
            {
                monitor.RecordUpdatedEvent -= crimeMonitorUpdated;
            }
        }

        private void crimeMonitorUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(  updateModifiersEstimateSummary  );
        }

        private void updateModifiersEstimateSummary()
        {
            var monitor = crimeMonitor();
            if ( monitor == null )
            {
                modifiersEstimateSummary.Visibility = Visibility.Collapsed;
                return;
            }

            var summaryLines = new List<string>();
            var bountyVoucherBonus = monitor.PowerplayBountyBonus;
            var finalBountyClaims = monitor.criminalrecord.Sum(c => c.bountyclaims);
            if ( bountyVoucherBonus is not null && finalBountyClaims > 0 )
            {
                var power = EDDI.Instance.GameState.CurrentStarSystem?.Power?.localizedName ?? string.Empty;
                var percent = (int)decimal.Round( bountyVoucherBonus.Value * 100, 0 );
                summaryLines.Add( string.Format(
                    CultureInfo.CurrentCulture,
                    Properties.CrimeMonitor.powerplay_bounty_boost_active,
                    power,
                    monitor.PledgedPowerRank,
                    percent,
                    finalBountyClaims ) );
            }

            var bountyReduction = monitor.PowerplayCrimeReduction;
            if ( bountyReduction is not null )
            {
                var power = EDDI.Instance.GameState.CurrentStarSystem?.Power?.localizedName ?? string.Empty;
                var percent = (int)decimal.Round( bountyReduction.Value * 100, 0 );
                var example = 100000 * (100 - percent) / 100;

                summaryLines.Add( string.Format(
                    CultureInfo.CurrentCulture,
                    Properties.CrimeMonitor.powerplay_bounty_reduction_active,
                    power,
                    monitor.PledgedPowerRank,
                    percent,
                    example ) );
            }

            modifiersEstimateSummary.Visibility = summaryLines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            modifiersEstimateText.Text = string.Join( Environment.NewLine, summaryLines );
        }

        private void addRecord(object sender, RoutedEventArgs e)
        {
            var record = new FactionRecord(Properties.CrimeMonitor.blank_faction);
            lock (CrimeMonitor.recordLock)
            {
                crimeMonitor()?.criminalrecord.Add(record);
            }
            crimeMonitor()?.writeRecord();
        }

        private void removeRecord(object sender, RoutedEventArgs e)
        {
            var record = (FactionRecord)((Button)e.Source).DataContext;
            crimeMonitor()?._RemoveRecord(record);
            crimeMonitor()?.writeRecord();
        }

        private async void updateRecord(object sender, RoutedEventArgs e)
        {
            if ( sender is Button updateButton && updateButton.DataContext is FactionRecord record )
            {
                if ( record.faction != Properties.CrimeMonitor.blank_faction )
                {
                    updateButton.Foreground = Brushes.Red;
                    updateButton.FontWeight = FontWeights.Bold;
                    updateButton.IsEnabled = false;

                    try
                    {
                        var Allegiance = Superpower.FromNameOrEdName( record.faction );
                        if ( Allegiance == null )
                        {
                            await crimeMonitor().GetFactionDataAsync( record, record.system );
                        }
                        else
                        {
                            record.Allegiance = Allegiance;
                        }
                        crimeMonitor()?.writeRecord();
                    }
                    catch ( OperationCanceledException )
                    {
                        // Task cancelled
                    }
                    finally
                    {
                        updateButton.Foreground = Brushes.Black;
                        updateButton.FontWeight = FontWeights.Regular;
                        updateButton.IsEnabled = true;
                    }
                }
            }
        }

        private void criminalRecordUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Source is DataGrid dataGrid && dataGrid.IsLoaded)
            {
                var record = (FactionRecord)dataGrid.CurrentItem;
                if (record != null)
                {
                    var column = dataGrid.CurrentColumn.DisplayIndex;
                    switch (column)
                    {
                        case 3: // Claims column
                            {
                                // Claims are final calculated values; programmatic claim adjustments are handled
                                // by FactionRecord.claims as final-value discrepancy reports.
                            }
                            break;
                        case 4: // Fines column
                            {
                                // All fines, including discrepancy report
                                var fines = record.factionReports
                                    .Where(r => !r.bounty && r.crimeDef != Crime.None)
                                    .Sum(r => r.amount);
                                if (record.fines != fines)
                                {
                                    // Create/modify 'discrepancy' report if total fines does not equal sum of fine reports
                                    var amount = record.fines - fines;
                                    var report = record.factionReports.FirstOrDefault(r => r.crimeDef == Crime.Fine);
                                    if (report == null)
                                    {
                                        report = new FactionReport(DateTime.UtcNow, false, Crime.Fine, null, 0);
                                        record.factionReports.Add(report);
                                    }
                                    report.amount += amount;
                                    if (report.amount == 0) { record.factionReports.Remove(report); }
                                }
                            }
                            break;
                        case 5: // Bounties column
                            {
                                // All bounties, including discrepancy report
                                var bounties = record.factionReports
                                    .Where(r => r.bounty && r.crimeDef != Crime.None)
                                    .Sum(r => r.amount);
                                if (record.bounties != bounties)
                                {
                                    // Create/modify 'discrepancy' report if total bounties does not equal sum of bounty reports
                                    var amount = record.bounties - bounties;
                                    var report = record.factionReports
                                        .FirstOrDefault(r => r.crimeDef == Crime.Bounty);
                                    if (report == null)
                                    {
                                        report = new FactionReport(DateTime.UtcNow, true, Crime.Bounty, null, 0);
                                        record.factionReports.Add(report);
                                    }
                                    report.amount += amount;
                                    if (report.amount == 0) { record.factionReports.Remove(report); }
                                }
                            }
                            break;
                    }
                }
            }
            // Update the crime monitor's information
            crimeMonitor()?.writeRecord();
            updateModifiersEstimateSummary();
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Swallow the character if it doesn't match the regex
            e.Handled = !GeneratedRegex.IsIntegerRegex().IsMatch( e.Text );
        }
    }
}
