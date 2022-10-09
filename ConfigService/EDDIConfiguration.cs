using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;

namespace EddiConfigService
{
    /// <summary>Configuration for EDDI</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\eddi.json")]
    public class EDDIConfiguration : Config
    {
        [JsonProperty("homeSystem")]
        public string HomeSystem { get; set; }

        [JsonProperty("homeStation")]
        public string HomeStation { get; set; }

        [JsonProperty("destinationSystem")]
        public string DestinationSystem { get; set; }

        [JsonProperty("squadronName")]
        public string SquadronName { get; set; }

        [JsonProperty("squadronID")]
        public string SquadronID { get; set; }

        [JsonProperty("squadronRank")]
        public string squadronRank
        {
            get => SquadronRank?.edname ?? "None";
            set
            {
                SquadronRank srDef = SquadronRank.FromEDName(value);
                this.SquadronRank = srDef;
            }
        }

        [JsonIgnore]
        public SquadronRank SquadronRank { get; set; } = SquadronRank.None;

        [JsonProperty("squadronAllegiance")]
        public string squadronAllegiance
        {

            get => SquadronAllegiance?.edname ?? Superpower.None.edname;
            set
            {
                Superpower saDef = Superpower.FromEDName(value);
                this.SquadronAllegiance = saDef;
            }
        }

        [JsonIgnore]
        public Superpower SquadronAllegiance { get; set; } = Superpower.None;

        [JsonProperty("squadronPower")]
        public string squadronPower
        {

            get => SquadronPower?.edname ?? Power.None.edname;
            set
            {
                Power spDef = Power.FromEDName(value);
                this.SquadronPower = spDef;
            }
        }

        [JsonIgnore]
        public Power SquadronPower { get; set; } = Power.None;

        [JsonProperty("squadronSystem")]
        public string SquadronSystem { get; set; }

        [JsonProperty("squadronFaction")]
        public string SquadronFaction { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("beta")]
        public bool Beta { get; set; }

        [JsonProperty("DisableTelemetry")]
        public bool DisableTelemetry { get; set; }

        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("powerMerits")]
        public int? powerMerits { get; set; }

        /// <summary>the current export target for the shipyard</summary>
        [JsonProperty("exporttarget")]
        public string exporttarget { get; set; }

        [JsonProperty("OverrideCulture")]
        public string OverrideCulture { get; set; }

        [JsonProperty("CommanderName")]
        public string CommanderName { get; set; }

        [JsonProperty("PhoneticName")]
        public string PhoneticName { get; set; }

        // Window Properties

        [JsonProperty("Maximized")]
        public bool Maximized { get; set; }

        [JsonProperty("Minimized")]
        public bool Minimized { get; set; }

        [JsonProperty("SelectedTab")]
        public int SelectedTab { get; set; }

        [JsonProperty("MainWindowPosition")]
        public Rect MainWindowPosition { get; set; }

        // Fleet Carrier
        public FleetCarrier fleetCarrier { get; set; }

        // Default
        public EDDIConfiguration()
        {
            Debug = false;
            Beta = false;
            Plugins = new Dictionary<string, bool>();
            exporttarget = "Coriolis";
            Gender = "Male";
            DisableTelemetry = false;

            // Window defaults
            Maximized = false;
            Minimized = false;
            SelectedTab = 0;
            MainWindowPosition = new Rect(40, 40, 800, 600);

            // Default the galnet monitor to 'off'
            Plugins.Add("Galnet monitor", false);
        }
    }
}
