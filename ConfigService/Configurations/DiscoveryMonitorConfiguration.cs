using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\discoverymonitor.json")]
    public class DiscoveryMonitorConfiguration : Config
    {
        public class Exobiology
        {
            public bool reportSlowBios;
            public bool reportDestinationBios;
            public bool reportBiosInSystemReport;
            public bool reportBiosAfterSystemScan;
            public int minimumBiosForReporting;

            public class Predictions
            {
                public bool skipCrystallineShards;
                public bool skipBarkMounds;
                public bool skipBrainTrees;
                public bool skipTubers;

                public Predictions() {
                    skipCrystallineShards = true;
                    skipBarkMounds = false;
                    skipBrainTrees = true;
                    skipTubers = false;
                }
            }
            public Predictions predictions;

            public class DataSold
            {
                public bool reportBreakdown;
                public bool reportTotalAlways;

                public DataSold() {
                    reportBreakdown = false;
                    reportTotalAlways = true;
                }
            }
            public DataSold dataSold;

            public class Scans
            {
                public bool reportBaseValue;
                public bool humanizeBaseValue;
                public bool reportBonusValue;
                public bool humanizeBonusValue;
                public bool reportSystem;
                public bool reportRegion;
                public bool reportLocation;
                public bool recommendOtherBios;
                public int reportGenusOnScan;
                public int reportSpeciesOnScan;
                public int reportConditionsOnScan;

                public Scans()
                {
                    reportBaseValue = true;
                    humanizeBaseValue = true;
                    reportBonusValue = true;
                    humanizeBonusValue = true;
                    reportSystem = true;
                    reportRegion = true;
                    reportLocation = false;
                    recommendOtherBios = true;
                    reportGenusOnScan = 0;
                    reportSpeciesOnScan = 0;
                    reportConditionsOnScan = 0;
                }
            }
            public Scans scans;

            public Exobiology()
            {
                predictions = new Predictions();
                dataSold = new DataSold();
                scans = new Scans();

                reportSlowBios = false;
                reportDestinationBios = true;
                reportBiosInSystemReport = true;
                reportBiosAfterSystemScan = true;
                minimumBiosForReporting = 0;
            }
        }

        public class Codex
        {
            public bool reportAllScans;
            public bool reportNewEntries;
            public bool reportNewTraits;
            public bool reportVoucherAmounts;
            public bool reportNewDetailsOnly;

            public class Astronomicals
            {
                public bool enable;
                public bool reportType;
                public bool reportDetails;

                public Astronomicals()
                {
                    enable = true;
                    reportType = true;
                    reportDetails = true;
                }
            }
            public Astronomicals astronomicals;

            public class Biologicals
            {
                public bool enable;
                public bool reportGenusDetails;
                public bool reportSpeciesDetails;
                public bool reportConditions;

                public Biologicals()
                {
                }
            }
            public Biologicals biologicals;

            public class Geologicals
            {
                public bool enable;
                public bool reportClass;
                public bool reportDetails;

                public Geologicals()
                {
                    enable = true;
                    reportClass = true;
                    reportDetails = true;
                }
            }
            public Geologicals geologicals;

            public class Guardian
            {
                public bool enable;
                public bool reportDetails;

                public Guardian()
                {
                    enable = true;
                    reportDetails = true;
                }
            }
            public Guardian guardian;

            public class Thargoid
            {
                public bool enable;
                public bool reportDetails;

                public Thargoid()
                {
                    enable = true;
                    reportDetails = true;
                }
            }
            public Thargoid thargoid;

            public Codex()
            {
                astronomicals = new Astronomicals();
                biologicals = new Biologicals();
                geologicals = new Geologicals();
                guardian = new Guardian();
                thargoid = new Thargoid();

                reportAllScans = false;
                reportNewEntries = true;
                reportNewTraits = true;
                reportVoucherAmounts = true;
                reportNewDetailsOnly = true;
            }
        }

        public Exobiology exobiology = new Exobiology();
        public Codex codex = new Codex();
    }
}
