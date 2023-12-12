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
        //public bool enableLogging;
        public Exobiology exobiology = new Exobiology();
        //public Codex codex = new Codex();

        public class Exobiology
        {
            //public bool reportSlowBios = false;
            //public bool reportDestinationBios = true;
            //public bool reportBiosInSystemReport = true;
            //public bool reportBiosAfterSystemScan = true;
            //public int minimumBiosForReporting = 0;

            public class Predictions
            {
                // Legacy
                public bool skipGroundStructIce = true;     // Crystalline Shards
                public bool skipCone = false;               // Bark Mounds
                public bool skipBrancae = true;             // Brain Trees
                public bool skipTubers = false;             // Sinuous Tubers

                // Odyssey
                public bool skipAleoids = false;            // Aleoida
                public bool skipVents = false;              // Amphora
                public bool skipSphere = false;             // Anemone
                public bool skipBacterial = false;          // Bacterium
                public bool skipCactoid = false;            // Cactoida
                public bool skipClypeus = false;            // Clypeus
                public bool skipConchas = false;            // Concha
                public bool skipElectricae = false;         // Electricae
                public bool skipFonticulus = false;         // Fonticulua
                public bool skipShrubs = false;             // Frutexa
                public bool skipFumerolas = false;          // Fumerola
                public bool skipFungoids = false;           // Fungoida
                public bool skipOsseus = false;             // Osseus
                public bool skipRecepta = false;            // Recepta
                public bool skipStratum = false;            // Stratum
                public bool skipTubus = false;              // Tubus
                public bool skipTussocks = false;           // Tussock

                // Non-predictable
                //public bool skipMineralSpheres = false;
                //public bool skipMetallicCrystals = false;
                //public bool skipSilicateCrystals = false;
                //public bool skipIceCrystals = false;
                //public bool skipMolluscReel = false;
                //public bool skipMolluscGlobe = false;
                //public bool skipMolluscBell = false;
                //public bool skipMolluscUmbrella = false;
                //public bool skipMolluscGourd = false;
                //public bool skipMolluscTorus = false;
                //public bool skipMolluscBulb = false;
                //public bool skipMolluscParasol = false;
                //public bool skipMolluscSquid = false;
                //public bool skipMolluscBullet = false;
                //public bool skipMolluscCapsule = false;
                //public bool skipCollaredPod = false;
                //public bool skipStolonPod = false;
                //public bool skipStolonTree = false;
                //public bool skipAsterPod = false;
                //public bool skipChalicePod = false;
                //public bool skipPedunclePod = false;
                //public bool skipRhizomePod = false;
                //public bool skipQuadripartitePod = false;
                //public bool skipVoidPod = false;
                //public bool skipAsterTree = false;
                //public bool skipPeduncleTree = false;
                //public bool skipGyreTree = false;
                //public bool skipGyrePod = false;
                //public bool skipVoidHeart = false;
                //public bool skipCalcitePlates = false;
                //public bool skipThargoidBarnacle = false;
            }
            public Predictions predictions = new Predictions();

        //    public class DataSold
        //    {
        //        public bool reportBreakdown = false;
        //        public bool reportTotalAlways = true;
        //    }
        //    public DataSold dataSold = new DataSold();

        //    public class Scans
        //    {
        //        public bool reportBaseValue = true;
        //        public bool humanizeBaseValue = true;
        //        public bool reportBonusValue = true;
        //        public bool humanizeBonusValue = true;
        //        public bool reportLocation = false;
        //        public bool recommendOtherBios = true;
        //        public int reportGenusOnScan = 0;
        //        public int reportSpeciesOnScan = 0;
        //        public int reportConditionsOnScan = 0;
        //    }
        //    public Scans scans = new Scans();
        }

        //public class Codex
        //{
        //    public bool reportAllScans = false;
        //    public bool reportNewEntries = true;
        //    public bool reportNewTraits = true;
        //    public bool reportVoucherAmounts = true;
        //    public bool reportNewDetailsOnly = true;
        //    public bool reportSystem = true;
        //    public bool reportRegion = true;

        //    public class Astronomicals
        //    {
        //        public bool enable = true;
        //        public bool reportType = true;
        //        public bool reportDetails = true;
        //    }
        //    public Astronomicals astronomicals = new Astronomicals();

        //    public class Biologicals
        //    {
        //        public bool enable;
        //        public bool reportGenusDetails;
        //        public bool reportSpeciesDetails;
        //        public bool reportConditions;
        //    }
        //    public Biologicals biologicals = new Biologicals();

        //    public class Geologicals
        //    {
        //        public bool enable = true;
        //        public bool reportClass = true;
        //        public bool reportDetails = true;
        //    }
        //    public Geologicals geologicals = new Geologicals();

        //    public class Guardian
        //    {
        //        public bool enable = true;
        //        public bool reportDetails = true;
        //    }
        //    public Guardian guardian = new Guardian();

        //    public class Thargoid
        //    {
        //        public bool enable = true;
        //        public bool reportDetails = true;
        //    }
        //    public Thargoid thargoid = new Thargoid();
        //}
    }
}
