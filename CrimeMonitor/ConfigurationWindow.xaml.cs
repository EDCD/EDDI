using Eddi;
using EddiDataDefinitions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace EddiCrimeMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private CrimeMonitor crimeMonitor()
        {
            return (CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            criminalRecord.ItemsSource = crimeMonitor()?.criminalrecord;

            CrimeMonitorConfiguration configuration = CrimeMonitorConfiguration.FromFile();
            crimeProfitShareInt.Text = configuration.profitShare?.ToString(CultureInfo.InvariantCulture);
        }

        private void addRecord(object sender, RoutedEventArgs e)
        {
            FactionRecord record = new FactionRecord(Properties.CrimeMonitor.blank_faction);
            crimeMonitor()?.criminalrecord.Add(record);
            crimeMonitor()?.writeRecord();
        }

        private void removeRecord(object sender, RoutedEventArgs e)
        {
            FactionRecord record = (FactionRecord)((Button)e.Source).DataContext;
            crimeMonitor()?._RemoveRecord(record);
            crimeMonitor()?.writeRecord();
        }

        private void updateRecord(object sender, RoutedEventArgs e)
        {
            FactionRecord record = (FactionRecord)((Button)e.Source).DataContext;
            if (record.faction != Properties.CrimeMonitor.blank_faction)
            {
                Button updateButton = (Button)sender;
                updateButton.Foreground = Brushes.Red;
                updateButton.FontWeight = FontWeights.Bold;

                Thread factionStationThread = new Thread(() =>
                {
                    crimeMonitor()?.GetFactionData(record, record.system);
                    Dispatcher?.Invoke(() =>
                    {
                        updateButton.Foreground = Brushes.Black;
                        updateButton.FontWeight = FontWeights.Regular;
                    });
                    crimeMonitor()?.writeRecord();
                })
                {
                    IsBackground = true
                };
                factionStationThread.Start();
            }
        }

        private void profitShareChanged(object sender, TextChangedEventArgs e)
        {
            CrimeMonitorConfiguration configuration = CrimeMonitorConfiguration.FromFile();
            try
            {
                int? profitShare = string.IsNullOrWhiteSpace(crimeProfitShareInt.Text) ? 0 : Convert.ToInt32(crimeProfitShareInt.Text, CultureInfo.InvariantCulture);
                crimeMonitor().profitShare = profitShare;
                configuration.profitShare = profitShare;
                configuration.ToFile();
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void criminalRecordUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Source is DataGrid)
            {
                FactionRecord record = (FactionRecord)((DataGrid)e.Source).CurrentItem;
                if (record != null)
                {
                    FactionReport report = new FactionReport();

                    int column = ((DataGrid)e.Source).CurrentColumn.DisplayIndex;
                    switch (column)
                    {
                        case 3: // Claims column
                            {
                                // All claims, including discrepancy report
                                long claims = record.factionReports
                                    .Where(r => r.crimeDef == Crime.None || r.crimeDef == Crime.Claim)
                                    .Sum(r => r.amount);
                                if (record.claims != claims)
                                {
                                    // Create/modify 'discrepancy' report if total claims does not equal sum of claim reports
                                    report = record.factionReports
                                        .FirstOrDefault(r => r.crimeDef == Crime.Claim);
                                    if (report == null)
                                    {
                                        report = new FactionReport(DateTime.UtcNow, false, 0, Crime.Claim, null, 0);
                                        record.factionReports.Add(report);
                                    }
                                    report.amount += record.claims - claims;
                                }
                            }
                            break;
                        case 4: // Fines column
                            {
                                // All fines, including discrepancy report
                                long fines = record.factionReports
                                    .Where(r => !r.bounty && r.crimeDef != Crime.None)
                                    .Sum(r => r.amount);
                                if (record.fines != fines)
                                {
                                    // Create/modify 'discrepancy' report if total fines does not equal sum of fine reports
                                    report = record.factionReports
                                        .FirstOrDefault(r => r.crimeDef == Crime.Fine);
                                    if (report == null)
                                    {
                                        report = new FactionReport(DateTime.UtcNow, false, 0, Crime.Fine, null, 0);
                                        record.factionReports.Add(report);
                                    }
                                    report.amount += record.fines - fines;
                                }
                            }
                            break;
                        case 5: // Bounties column
                            {
                                // All bounties, including discrepancy report
                                long bounties = record.factionReports
                                    .Where(r => r.bounty && r.crimeDef != Crime.None)
                                    .Sum(r => r.amount);
                                if (record.bounties != bounties)
                                {
                                    // Create/modify 'discrepancy' report if total bounties does not equal sum of bonty reports
                                    report = record.factionReports
                                        .FirstOrDefault(r => r.crimeDef == Crime.Bounty);
                                    if (report == null)
                                    {
                                        report = new FactionReport(DateTime.UtcNow, true, 0, Crime.Bounty, null, 0);
                                        record.factionReports.Add(report);
                                    }
                                    report.amount += record.bounties - bounties;
                                }
                            }
                            break;
                    }
                }
            }
            // Update the crime monitor's information
            crimeMonitor()?.writeRecord();
        }
    }
}
