using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public int subslot { get; private set; }
        public string loadout { get; private set; }
        public int rebuilds { get; private set; }

        [JsonIgnore]
        public string localizedName => definition.localizedName;

        [JsonIgnore]
        public string invariantName => definition.invariantName;

        [JsonIgnore]
        [Obsolete("Please be explicit and use localizedName or invariantName")]
        public string name => localizedName;
        
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

        public static Vehicle FromJson(int subslot, dynamic json)
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
