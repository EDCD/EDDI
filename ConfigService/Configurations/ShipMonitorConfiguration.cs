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
        public int? currentshipid { get; set; }

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
        }private ImmutableList<Ship> _shipyard = ImmutableList.Create<Ship>();
        private readonly object _shipyardLock = new object();

        public List<StoredModule> storedmodules { get; set; } = new List<StoredModule>();

        /// <summary>the current export target for the shipyard</summary>
        public string exporttarget { get; set; } = "Coriolis";

        public decimal insurance { get; set; } = 0.05M;

        public DateTime updatedat { get; set; } = DateTime.MinValue;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (!shipyard.Any())
            {
                // Used to be in a separate 'ships' file so try that to allow migration
                var oldFilename = Constants.DATA_DIR + @"\ships.json";
                if (File.Exists(oldFilename))
                {
                    try
                    {
                        string oldData = Files.Read(oldFilename);
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
