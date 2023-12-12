using EddiCore;
using EddiConfigService;
using System;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace EddiDiscoveryMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private DiscoveryMonitor discoveryMonitor ()
        {
            return (DiscoveryMonitor)EDDI.Instance.ObtainMonitor( "Discovery Monitor" );
        }

        public ConfigurationWindow ()
        {
            InitializeComponent();

            var configuration = ConfigService.Instance.discoveryMonitorConfiguration;

            checkboxIgnoreBrainTrees.IsChecked = configuration.exobiology.predictions.skipBrancae;
            checkboxIgnoreCrystalShards.IsChecked = configuration.exobiology.predictions.skipGroundStructIce;
            checkboxIgnoreBarkMounds.IsChecked = configuration.exobiology.predictions.skipCone;
            checkboxIgnoreTubers.IsChecked = configuration.exobiology.predictions.skipTubers;

            //exobioSlowBios.IsChecked = configuration.exobiology.reportSlowBios;
            //exobioMinimumBios.Text = configuration.exobiology.minimumBiosForReporting.ToString();
            //exobioReportDest.IsChecked = configuration.exobiology.reportDestinationBios;
            //exobioSystemReport.IsChecked = configuration.exobiology.reportBiosInSystemReport;
            //exobioSystemScan.IsChecked = configuration.exobiology.reportBiosAfterSystemScan;
            //exobioSoldBreakdown.IsChecked = configuration.exobiology.dataSold.reportBreakdown;
            //exobioSoldReportTotal.IsChecked = configuration.exobiology.dataSold.reportTotalAlways;
            //exobioScansReportBase.IsChecked = configuration.exobiology.scans.reportBaseValue;
            //exobioScansHumanizeBase.IsChecked = configuration.exobiology.scans.humanizeBaseValue;
            //exobioScansReportBonus.IsChecked = configuration.exobiology.scans.reportBonusValue;
            //exobioScansHumanizeBonus.IsChecked = configuration.exobiology.scans.humanizeBonusValue;
            //exobioScansReportLocation.IsChecked = configuration.exobiology.scans.reportLocation;
            //exobioScansRecommendBodies.IsChecked = configuration.exobiology.scans.recommendOtherBios;
            //exobioScansGenusNum.Text = configuration.exobiology.scans.reportGenusOnScan.ToString();
            //exobioScansSpeciesNum.Text = configuration.exobiology.scans.reportSpeciesOnScan.ToString();
            //exobioScansConditionsNum.Text = configuration.exobiology.scans.reportConditionsOnScan.ToString();

            //codexReportAllScans.IsChecked = configuration.codex.reportAllScans;
            //codexReportNewEntries.IsChecked = configuration.codex.reportNewEntries;
            //codexReportNewTraits.IsChecked = configuration.codex.reportNewTraits;
            //codexReportVoucherAmount.IsChecked = configuration.codex.reportVoucherAmounts;
            //codexReportNewOnly.IsChecked = configuration.codex.reportNewDetailsOnly;
            //codexAstroEnable.IsChecked = configuration.codex.astronomicals.enable;
            //codexAstroType.IsChecked = configuration.codex.astronomicals.reportType;
            //codexAstroDetails.IsChecked = configuration.codex.astronomicals.reportDetails;
            //codexBioEnable.IsChecked = configuration.codex.biologicals.enable;
            //codexBioGenus.IsChecked = configuration.codex.biologicals.reportGenusDetails;
            //codexBioSpecies.IsChecked = configuration.codex.biologicals.reportSpeciesDetails;
            //codexBioConditions.IsChecked = configuration.codex.biologicals.reportConditions;
            //codexGeoEnable.IsChecked = configuration.codex.geologicals.enable;
            //codexGeoClass.IsChecked = configuration.codex.geologicals.reportClass;
            //codexGeoDetails.IsChecked = configuration.codex.geologicals.reportDetails;
            //codexGuardianEnable.IsChecked = configuration.codex.guardian.enable;
            //codexGuardianDetails.IsChecked = configuration.codex.guardian.reportDetails;
            //codexThargoidEnable.IsChecked = configuration.codex.thargoid.enable;
            //codexThargoidDetails.IsChecked = configuration.codex.thargoid.reportDetails;
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }
        
        // ########################################
        //      Exobiology
        // ########################################
        private void IgnoreBrainTrees_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            configuration.exobiology.predictions.skipBrancae = checkboxIgnoreBrainTrees.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            discoveryMonitor()?.Reload();
        }

        private void IgnoreCrystalShards_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            configuration.exobiology.predictions.skipGroundStructIce = checkboxIgnoreCrystalShards.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            discoveryMonitor()?.Reload();
        }

        private void IgnoreBarkMounds_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            configuration.exobiology.predictions.skipCone = checkboxIgnoreBarkMounds.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            discoveryMonitor()?.Reload();
        }

        private void IgnoreTubers_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            configuration.exobiology.predictions.skipTubers = checkboxIgnoreTubers.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            discoveryMonitor()?.Reload();
        }

        private void exobioSlowBios_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.reportSlowBios = exobioSlowBios.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioMinimumBios_Changed ( object sender, TextChangedEventArgs e )
        {
            //try
            //{
            //    var configuration = ConfigService.Instance.discoveryMonitorConfiguration;

            //    int? minimumBios = string.IsNullOrWhiteSpace(exobioMinimumBios.Text) ? 0
            //            : Convert.ToInt32(exobioMinimumBios.Text/*, CultureInfo.InvariantCulture*/);

            //    configuration.exobiology.minimumBiosForReporting = (int)minimumBios;
            //    ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //    discoveryMonitor()?.Reload();
            //}
            //catch
            //{
            //    // Bad user input; ignore it
            //}
        }

        private void exobioReportDest_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.reportDestinationBios = exobioReportDest.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioSystemReport_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.reportBiosInSystemReport = exobioSystemReport.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioSystemScan_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.reportBiosAfterSystemScan = exobioSystemScan.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioSoldBreakdown_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.dataSold.reportBreakdown = exobioSoldBreakdown.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioSoldReportTotal_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.dataSold.reportTotalAlways = exobioSoldReportTotal.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansReportBase_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.reportBaseValue = exobioScansReportBase.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansHumanizeBase_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.humanizeBaseValue = exobioScansHumanizeBase.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansReportBonus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.reportBonusValue = exobioScansReportBonus.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansHumanizeBonus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.humanizeBonusValue = exobioScansHumanizeBonus.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansReportLocation_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.reportLocation = exobioScansReportLocation.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansRecommendBodies_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.exobiology.scans.recommendOtherBios = exobioScansRecommendBodies.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void exobioScansGenusNum_Changed ( object sender, TextChangedEventArgs e )
        {
            //try
            //{
            //    var configuration = ConfigService.Instance.discoveryMonitorConfiguration;

            //    int? genusNum = string.IsNullOrWhiteSpace(exobioScansGenusNum.Text) ? 0
            //            : Convert.ToInt32(exobioScansGenusNum.Text/*, CultureInfo.InvariantCulture*/);

            //    configuration.exobiology.scans.reportGenusOnScan = (int)genusNum;
            //    ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //    discoveryMonitor()?.Reload();
            //}
            //catch
            //{
            //    // Bad user input; ignore it
            //}
        }

        private void exobioScansSpeciesNum_Changed ( object sender, TextChangedEventArgs e )
        {
            //try
            //{
            //    var configuration = ConfigService.Instance.discoveryMonitorConfiguration;

            //    int? speciesNum = string.IsNullOrWhiteSpace(exobioScansSpeciesNum.Text) ? 0
            //            : Convert.ToInt32(exobioScansSpeciesNum.Text/*, CultureInfo.InvariantCulture*/);

            //    configuration.exobiology.scans.reportSpeciesOnScan = (int)speciesNum;
            //    ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //    discoveryMonitor()?.Reload();
            //}
            //catch
            //{
            //    // Bad user input; ignore it
            //}
        }

        private void exobioScansConditionsNum_Changed ( object sender, TextChangedEventArgs e )
        {
            //try
            //{
            //    var configuration = ConfigService.Instance.discoveryMonitorConfiguration;

            //    int? conditionsNum = string.IsNullOrWhiteSpace(exobioScansConditionsNum.Text) ? 0
            //            : Convert.ToInt32(exobioScansConditionsNum.Text/*, CultureInfo.InvariantCulture*/);

            //    configuration.exobiology.scans.reportConditionsOnScan = (int)conditionsNum;
            //    ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //    discoveryMonitor()?.Reload();
            //}
            //catch
            //{
            //    // Bad user input; ignore it
            //}
        }
        
        // ########################################
        //      Codex Entries
        // ########################################
        private void codexReportAllScans_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportAllScans = codexReportAllScans.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportNewEntries_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportNewEntries = codexReportNewEntries.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportNewTraits_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportNewTraits = codexReportNewTraits.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportVoucherAmount_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportVoucherAmounts = codexReportVoucherAmount.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportNewOnly_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportNewDetailsOnly = codexReportNewOnly.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportSystem_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportSystem = codexReportSystem.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexReportRegion_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.reportRegion = codexReportRegion.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexAstroEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.astronomicals.enable = codexAstroEnable.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexAstroType_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.astronomicals.reportType = codexAstroType.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexAstroDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.astronomicals.reportDetails = codexAstroDetails.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexBioEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.biologicals.enable = codexBioEnable.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexBioGenus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.biologicals.reportGenusDetails = codexBioGenus.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexBioSpecies_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.biologicals.reportSpeciesDetails = codexBioSpecies.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexBioConditions_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.biologicals.reportConditions = codexBioConditions.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexGeoEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.geologicals.enable = codexGeoEnable.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexGeoClass_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.geologicals.reportClass = codexGeoClass.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexGeoDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.geologicals.reportDetails = codexGeoDetails.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexGuardianEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.guardian.enable = codexGuardianEnable.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexGuardianDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.guardian.reportDetails = codexGuardianDetails.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexThargoidEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.thargoid.enable = codexThargoidEnable.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }

        private void codexThargoidDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            //var configuration = ConfigService.Instance.discoveryMonitorConfiguration;
            //configuration.codex.thargoid.reportDetails = codexThargoidDetails.IsChecked ?? false;
            //ConfigService.Instance.discoveryMonitorConfiguration = configuration;
            //discoveryMonitor()?.Reload();
        }
    }
}
