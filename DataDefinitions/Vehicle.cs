using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public int subslot { get; set; }
        public string loadout { get; set; }
        public int rebuilds { get; set; }

        [JsonIgnore]
        private VehicleDefinition definition;

        public static Vehicle FromLoadoutName(string loadout)
        {
            if (loadout == null) { return null; }

            string tidiedLoadout = loadout.ToLowerInvariant()
                .Replace("_fighter_loadout", "")
                .Replace("_name", "");
            List<string> parts = tidiedLoadout.Split('_').ToList();

            Vehicle vehicle = new Vehicle();
            if (parts.Count == 2)
            {
                vehicle.definition = VehicleDefinition.FromEDName(parts[0]);
                vehicle.loadout = parts[1];
            }

            return vehicle;
        }

        public static Vehicle FromJson(int subslot, JObject json)
        {
            if (json is null) { return null; }

            string edName = (string)json["EDName"];
            Vehicle vehicle = new Vehicle()
            {
                loadout = (string)json["loadout"],
                rebuilds = (int)json["rebuilds"],
                subslot = subslot,
                definition = VehicleDefinition.FromEDName(edName),
            };

            return vehicle;
        }

    }
}
