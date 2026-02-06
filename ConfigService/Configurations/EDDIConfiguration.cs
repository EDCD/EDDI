using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for EDDI</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\eddi.json")]
    public class EDDIConfiguration : Config
    {
        [ JsonProperty( "destinationSystem" ) ]
        public string DestinationSystem
        {
            get => _destinationSystem;
            set
            {
                if ( value == _destinationSystem )
                {
                    return;
                }

                _destinationSystem = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "destinationSystemAddress" ) ]
        public ulong? DestinationSystemAddress
        {
            get => _destinationSystemAddress;
            set
            {
                if ( value == _destinationSystemAddress )
                {
                    return;
                }

                _destinationSystemAddress = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "debug" ) ]
        public bool VerboseLogging
        {
            get => _verboseLogging;
            set
            {
                if ( value == _verboseLogging )
                {
                    return;
                }

                _verboseLogging = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "beta" ) ]
        public bool AcceptsBetaReleases
        {
            get => _acceptsBetaReleases;
            set
            {
                if ( value == _acceptsBetaReleases )
                {
                    return;
                }

                _acceptsBetaReleases = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "DisableTelemetry" ) ]
        public bool DisableTelemetry
        {
            get => _disableTelemetry;
            set
            {
                if ( value == _disableTelemetry )
                {
                    return;
                }

                _disableTelemetry = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "plugins" ) ]
        public IDictionary<string, bool> Plugins
        {
            get => _plugins;
            set
            {
                if ( Equals( value, _plugins ) )
                {
                    return;
                }

                _plugins = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "OverrideCulture" ) ]
        public string OverrideCulture
        {
            get => _overrideCulture;
            set
            {
                if ( value == _overrideCulture )
                {
                    return;
                }

                _overrideCulture = value;
                OnPropertyChanged();
            }
        }

        // Window Properties

        [ JsonProperty( "Maximized" ) ]
        public bool Maximized
        {
            get => _maximized;
            set
            {
                if ( value == _maximized )
                {
                    return;
                }

                _maximized = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "Minimized" ) ]
        public bool Minimized
        {
            get => _minimized;
            set
            {
                if ( value == _minimized )
                {
                    return;
                }

                _minimized = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "SelectedTab" ) ]
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if ( value == _selectedTab )
                {
                    return;
                }

                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "MainWindowPosition" ) ]
        public Rect MainWindowPosition
        {
            get => _mainWindowPosition;
            set
            {
                if ( value.Equals( _mainWindowPosition ) )
                {
                    return;
                }

                _mainWindowPosition = value;
                OnPropertyChanged();
            }
        }

        // Hotkeys

        [JsonProperty( "Hotkeys" )] 
        private ImmutableDictionary<string, string> Hotkeys = ImmutableDictionary<string, string>.Empty;

        private string _destinationSystem;
        private ulong? _destinationSystemAddress;
        private bool _verboseLogging;
        private bool _acceptsBetaReleases;
        private bool _disableTelemetry;
        private IDictionary<string, bool> _plugins;
        private string _overrideCulture;
        private bool _maximized;
        private bool _minimized;
        private int _selectedTab;
        private Rect _mainWindowPosition;
        public ImmutableDictionary<string, string> GetHotkeysCopy () => Hotkeys;
        public void AddHotkey ( string name, string gesture ) => Hotkeys = Hotkeys.Add( name, gesture );
        public void RemoveHotkey ( string name ) => Hotkeys = Hotkeys.Remove( name );

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
