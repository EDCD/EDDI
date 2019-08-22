using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public int subslot { get; private set; }
        public string loadout { get; private set; }
        public int rebuilds { get; private set; }

        [JsonIgnore]
        private VehicleDefinition definition;

        [JsonIgnore]
        public string localizedName => definition?.localizedName;

        [JsonIgnore]
        public string invariantName => definition?.invariantName;

        [JsonIgnore]
        [Obsolete("Please be explicit and use localizedName or invariantName")]
        public string name => localizedName;

        [JsonIgnore]
        private LoadoutDescription loadoutDescription;

        [JsonIgnore]
        public string localizedDescription => loadoutDescription?.localizedName;

        [JsonIgnore]
        [Obsolete("Please be explicit and use localizedDescription")]
        public string description => localizedDescription;

        public static Vehicle FromJson(int subslot, dynamic json)
        {
            if (json is null) { return null; }

            string edName = (string)json["name"];
            string loadoutName = (string)json["loadoutName"];

            Vehicle vehicle = new Vehicle()
            {
                loadout = (string)json["loadout"],
                rebuilds = (int)json["rebuilds"],
                subslot = subslot,
                definition = VehicleDefinition.FromEDName(edName),
                loadoutDescription = LoadoutDescription.FromLoadoutName(loadoutName)
            };

            return vehicle;
        }

    }
}
