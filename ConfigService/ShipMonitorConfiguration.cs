using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Utilities;

namespace EddiConfigService
{
    /// <summary>Storage for ship and shipyard information</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\shipmonitor.json")]
    public class ShipMonitorConfiguration : Config
    {
        public int? currentshipid { get; set; }
        public ObservableCollection<Ship> shipyard { get; set; } = new ObservableCollection<Ship>();
        public List<StoredModule> storedmodules { get; set; } = new List<StoredModule>();
        public DateTime updatedat { get; set; } = DateTime.MinValue;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (!shipyard.Any())
            {
                // Used to be in a separate 'ships' file so try that to allow migration
                string oldFilename = Constants.DATA_DIR + @"\ships.json";
                if (File.Exists(oldFilename))
                {
                    try
                    {
                        string oldData = Files.Read(oldFilename);
                        if (oldData != null)
                        {
                            var oldShipsConfiguration = JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<Ship>>>(oldData);
                            // At this point the old file is confirmed to have been there - migrate it
                            // There was a bug that caused null entries to be written to the ships configuration; remove these if present
                            var oldShips = new ObservableCollection<Ship>(oldShipsConfiguration?["ships"]?.Where(x => x.Role != null) ?? new List<Ship>());
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
            foreach (Ship ship in shipyard)
            {
                ship.Augment();
            }
        }
    }
}
