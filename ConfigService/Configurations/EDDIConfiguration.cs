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
        public bool VerboseLogging { get; set; }

        [JsonProperty("beta")]
        public bool AcceptsBetaReleases { get; set; }

        [JsonProperty("DisableTelemetry")]
        public bool DisableTelemetry { get; set; }

        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }

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

        // Hotkeys

        [ JsonProperty( "Hotkeys" ) ]
        public Dictionary<string, string> Hotkeys { get; set; } = new Dictionary<string, string>();

        // Default
        public EDDIConfiguration()
        {
            VerboseLogging = false;
            AcceptsBetaReleases = false;
            Plugins = new Dictionary<string, bool>();
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
