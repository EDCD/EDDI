using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\discoverymonitor.json")]
    public class DiscoveryMonitorConfiguration : Config
    {
        // Enables the debugging messages I used to implement codex and exobiology features
        // Still requires the overall verbose logging to be enabled for EDDI
        // This is not available everywhere so a few things always show up with verbose logging
        public bool enableLogging;
        public bool enableVariantPredictions;
        public Exobiology exobiology = new Exobiology();
        public Codex codex = new Codex();

        public class Exobiology
        {
            public bool reportSlowBios = false;
            public bool reportDestinationBios = true;
            public bool reportBiosInSystemReport = true;
            public bool reportBiosAfterSystemScan = true;
            public int minimumBiosForReporting = 0;

            public class Predictions
            {
                public bool skipCrystallineShards = true;
                public bool skipBarkMounds = false;
                public bool skipBrainTrees = true;
                public bool skipTubers = false;
            }
            public Predictions predictions = new Predictions();

            public class DataSold
            {
                public bool reportBreakdown = false;
                public bool reportTotalAlways = true;
            }
            public DataSold dataSold = new DataSold();

            public class Scans
            {
                public bool reportBaseValue = true;
                public bool humanizeBaseValue = true;
                public bool reportBonusValue = true;
                public bool humanizeBonusValue = true;
                public bool reportLocation = false;
                public bool recommendOtherBios = true;
                public int reportGenusOnScan = 0;
                public int reportSpeciesOnScan = 0;
                public int reportConditionsOnScan = 0;
            }
            public Scans scans = new Scans();
        }

        public class Codex
        {
            public bool reportAllScans = false;
            public bool reportNewEntries = true;
            public bool reportNewTraits = true;
            public bool reportVoucherAmounts = true;
            public bool reportNewDetailsOnly = true;
            public bool reportSystem = true;
            public bool reportRegion = true;

            public class Astronomicals
            {
                public bool enable = true;
                public bool reportType = true;
                public bool reportDetails = true;
            }
            public Astronomicals astronomicals = new Astronomicals();

            public class Biologicals
            {
                public bool enable;
                public bool reportGenusDetails;
                public bool reportSpeciesDetails;
                public bool reportConditions;
            }
            public Biologicals biologicals = new Biologicals();

            public class Geologicals
            {
                public bool enable = true;
                public bool reportClass = true;
                public bool reportDetails = true;
            }
            public Geologicals geologicals = new Geologicals();

            public class Guardian
            {
                public bool enable = true;
                public bool reportDetails = true;
            }
            public Guardian guardian = new Guardian();

            public class Thargoid
            {
                public bool enable = true;
                public bool reportDetails = true;
            }
            public Thargoid thargoid = new Thargoid();
        }
    }
}
