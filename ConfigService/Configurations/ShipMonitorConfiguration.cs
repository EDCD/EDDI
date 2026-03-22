using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Utilities;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for ship and shipyard information</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\shipmonitor.json")]
    public class ShipMonitorConfiguration : Config
    {
        private ImmutableList<Ship> _shipyard = ImmutableList.Create<Ship>();
        private readonly object _shipyardLock = new();
        private DateTime _updatedat = DateTime.MinValue;
        private decimal _insurance = 0.05M;
        private string _exporttarget = "Coriolis";
        private List<StoredModule> _storedmodules = new();
        private int? _currentshipid;
        
        public int? currentshipid
        {
            get => _currentshipid;
            set
            {
                if ( value == _currentshipid )
                {
                    return;
                }

                _currentshipid = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns an immutable list of ships in the shipyard.
        /// </summary>
        public ImmutableList<Ship> shipyard
        {
            get
            {
                lock ( _shipyardLock )
                {
                    return _shipyard;
                }
            }
            set
            {
                lock ( _shipyardLock )
                {
                    _shipyard = value;
                }
            }
        }

        public List<StoredModule> storedmodules
        {
            get => _storedmodules;
            set
            {
                if ( Equals( value, _storedmodules ) )
                {
                    return;
                }

                _storedmodules = value;
                OnPropertyChanged();
            }
        }

        /// <summary>the current export target for the shipyard</summary>
        public string exporttarget
        {
            get => _exporttarget;
            set
            {
                if ( value == _exporttarget )
                {
                    return;
                }

                _exporttarget = value;
                OnPropertyChanged();
            }
        }

        public decimal insurance
        {
            get => _insurance;
            set
            {
                if ( value == _insurance )
                {
                    return;
                }

                _insurance = value;
                OnPropertyChanged();
            }
        }

        public DateTime updatedat
        {
            get => _updatedat;
            set
            {
                if ( value.Equals( _updatedat ) )
                {
                    return;
                }

                _updatedat = value;
                OnPropertyChanged();
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (shipyard.IsEmpty)
            {
                // Used to be in a separate 'ships' file so try that to allow migration
                var oldFilename = Constants.DATA_DIR + @"\ships.json";
                if (File.Exists(oldFilename))
                {
                    try
                    {
                        var oldData = Files.Read(oldFilename);
                        if (oldData != null)
                        {
                            var oldShipsConfiguration = JsonConvert.DeserializeObject<Dictionary<string, List<Ship>>>(oldData);
                            // At this point the old file is confirmed to have been there - migrate it
                            // There was a bug that caused null entries to be written to the ships configuration; remove these if present
                            var oldShips = ImmutableList.Create(oldShipsConfiguration?["ships"]?.Where(x => x.Role != null).ToArray() ?? Array.Empty<Ship>() );
                            shipyard = oldShips;
                            File.Delete(oldFilename);
                        }
                    }
                    catch (Exception ex)
                    {
                        // There was a problem parsing the old file, just press on
                        Logging.Error(ex.Message, ex);
                    }
                }
            }

            // Populate static information from definitions
            foreach (var ship in shipyard)
            {
                ship.Augment();
            }
        }
    }
}
