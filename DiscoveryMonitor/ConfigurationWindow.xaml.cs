using EddiConfigService;
using EddiCore;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace EddiDiscoveryMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private DiscoveryMonitor discoveryMonitor ()
        {
            return (DiscoveryMonitor)EDDI.Instance.ObtainMonitor( "Discovery monitor" );
        }

        public ConfigurationWindow ()
        {
            InitializeComponent();
        }

        private void IgnoreBrainTrees_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var predictions = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions;
            predictions.skipBrainTrees = checkboxIgnoreBrainTrees.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions = predictions;
        }

        private void IgnoreCrystalShards_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var predictions = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions;
            predictions.skipCrystallineShards = checkboxIgnoreCrystalShards.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions = predictions;
        }

        private void IgnoreBarkMounds_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var predictions = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions;
            predictions.skipBarkMounds = checkboxIgnoreBarkMounds.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions = predictions;
        }

        private void IgnoreTubers_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var predictions = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions;
            predictions.skipTubers = checkboxIgnoreTubers.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.predictions = predictions;
        }

        private void exobioSlowBios_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var exobiology = ConfigService.Instance.discoveryMonitorConfiguration.exobiology;
            exobiology.reportSlowBios = exobioSlowBios.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology = exobiology;
        }

        private void exobioMinimumBios_Preview ( object sender, System.Windows.Input.TextCompositionEventArgs e )
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch( e.Text );
        }

        private void exobioMinimumBios_Changed ( object sender, TextChangedEventArgs e )
        {
            var exobiology = ConfigService.Instance.discoveryMonitorConfiguration.exobiology;
            exobiology.minimumBiosForReporting = int.Parse( exobioMinimumBios.Text );
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology = exobiology;
        }

        private void exobioReportDest_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var exobiology = ConfigService.Instance.discoveryMonitorConfiguration.exobiology;
            exobiology.reportDestinationBios = exobioReportDest.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology = exobiology;
        }

        private void exobioSystemReport_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var exobiology = ConfigService.Instance.discoveryMonitorConfiguration.exobiology;
            exobiology.reportBiosInSystemReport = exobioSystemReport.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology = exobiology;
        }

        private void exobioSystemScan_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var exobiology = ConfigService.Instance.discoveryMonitorConfiguration.exobiology;
            exobiology.reportBiosAfterSystemScan = exobioSystemScan.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology = exobiology;
        }

        private void exobioSoldBreakdown_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var dataSold = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.dataSold;
            dataSold.reportBreakdown = exobioSoldBreakdown.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.dataSold = dataSold;
        }

        private void exobioSoldReportTotal_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var dataSold = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.dataSold;
            dataSold.reportTotalAlways = exobioSoldReportTotal.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.dataSold = dataSold;
        }

        private void exobioScansReportBase_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportBaseValue = exobioScansReportBase.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansHumanizeBase_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.humanizeBaseValue = exobioScansHumanizeBase.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansReportBonus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportBonusValue = exobioScansReportBonus.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansHumanizeBonus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.humanizeBonusValue = exobioScansHumanizeBonus.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansReportLocation_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportLocation = exobioScansReportLocation.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansRecommendBodies_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.recommendOtherBios = exobioScansRecommendBodies.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansGenusNum_Preview ( object sender, System.Windows.Input.TextCompositionEventArgs e )
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch( e.Text );
        }

        private void exobioScansGenusNum_Changed ( object sender, TextChangedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportGenusOnScan = int.Parse( exobioScansGenusNum.Text );
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansSpeciesNum_Preview ( object sender, System.Windows.Input.TextCompositionEventArgs e )
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch( e.Text );
        }

        private void exobioScansSpeciesNum_Changed ( object sender, TextChangedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportSpeciesOnScan = int.Parse( exobioScansSpeciesNum.Text );
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }

        private void exobioScansConditionsNum_Preview ( object sender, System.Windows.Input.TextCompositionEventArgs e )
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch( e.Text );
        }

        private void exobioScansConditionsNum_Changed ( object sender, TextChangedEventArgs e )
        {
            var scans = ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans;
            scans.reportConditionsOnScan = int.Parse( exobioScansConditionsNum.Text );
            ConfigService.Instance.discoveryMonitorConfiguration.exobiology.scans = scans;
        }






        private void codexReportAllScans_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var codex = ConfigService.Instance.discoveryMonitorConfiguration.codex;
            codex.reportAllScans = codexReportAllScans.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex = codex;
        }

        private void codexReportNewEntries_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var codex = ConfigService.Instance.discoveryMonitorConfiguration.codex;
            codex.reportNewEntries = codexReportNewEntries.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex = codex;
        }

        private void codexReportNewTraits_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var codex = ConfigService.Instance.discoveryMonitorConfiguration.codex;
            codex.reportNewTraits = codexReportNewTraits.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex = codex;
        }

        private void codexReportVoucherAmount_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var codex = ConfigService.Instance.discoveryMonitorConfiguration.codex;
            codex.reportVoucherAmounts = codexReportVoucherAmount.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex = codex;
        }

        private void codexReportNewOnly_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var codex = ConfigService.Instance.discoveryMonitorConfiguration.codex;
            codex.reportNewDetailsOnly = codexReportNewOnly.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex = codex;
        }

        private void codexAstroEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var astronomicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals;
            astronomicals.enable = codexAstroEnable.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals = astronomicals;
        }

        private void codexAstroType_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var astronomicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals;
            astronomicals.reportType = codexAstroType.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals = astronomicals;
        }

        private void codexAstroDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var astronomicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals;
            astronomicals.reportDetails = codexAstroDetails.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.astronomicals = astronomicals;
        }

        private void codexBioEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var biologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals;
            biologicals.enable = codexBioEnable.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals = biologicals;
        }

        private void codexBioGenus_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var biologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals;
            biologicals.reportGenusDetails = codexBioGenus.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals = biologicals;
        }

        private void codexBioSpecies_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var biologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals;
            biologicals.reportSpeciesDetails = codexBioSpecies.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals = biologicals;
        }

        private void codexBioConditions_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var biologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals;
            biologicals.reportConditions = codexBioConditions.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.biologicals = biologicals;
        }

        private void codexGeoEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var geologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals;
            geologicals.enable = codexGeoEnable.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals = geologicals;
        }

        private void codexGeoClass_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var geologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals;
            geologicals.reportClass = codexGeoClass.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals = geologicals;
        }

        private void codexGeoDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var geologicals = ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals;
            geologicals.reportDetails = codexGeoDetails.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.geologicals = geologicals;
        }

        private void codexGuardianEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var guardian = ConfigService.Instance.discoveryMonitorConfiguration.codex.guardian;
            guardian.enable = codexGuardianEnable.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.guardian = guardian;
        }

        private void codexGuardianDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var guardian = ConfigService.Instance.discoveryMonitorConfiguration.codex.guardian;
            guardian.reportDetails = codexGuardianDetails.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.guardian = guardian;
        }

        private void codexThargoidEnable_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var thargoid = ConfigService.Instance.discoveryMonitorConfiguration.codex.thargoid;
            thargoid.enable = codexThargoidEnable.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.thargoid = thargoid;
        }

        private void codexThargoidDetails_Toggle ( object sender, System.Windows.RoutedEventArgs e )
        {
            var thargoid = ConfigService.Instance.discoveryMonitorConfiguration.codex.thargoid;
            thargoid.reportDetails = codexThargoidDetails.IsChecked ?? false;
            ConfigService.Instance.discoveryMonitorConfiguration.codex.thargoid = thargoid;
        }
    }
}
