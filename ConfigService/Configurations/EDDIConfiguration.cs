using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for EDDI</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\eddi.json")]
    public class EDDIConfiguration : Config
    {
        [JsonProperty("destinationSystem")]
        public string DestinationSystem { get; set; }

        [JsonProperty( "destinationSystemAddress" )]
        public ulong? DestinationSystemAddress { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("beta")]
        public bool Beta { get; set; }

        [JsonProperty("DisableTelemetry")]
        public bool DisableTelemetry { get; set; }

        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }

        /// <summary>the current export target for the shipyard</summary>
        [JsonProperty("exporttarget")]
        public string exporttarget { get; set; }

        [JsonProperty("OverrideCulture")]
        public string OverrideCulture { get; set; }

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
