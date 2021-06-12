using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public int subslot { get; private set; }

        [PublicAPI]
        public string loadout { get; private set; }

        [PublicAPI]
        public int rebuilds { get; private set; }

        public string vehicleDefinition
        {
            get => vehicleDef?.edname;
            set
            {
                VehicleDefinition vDef = VehicleDefinition.FromEDName(value);
                vehicleDef = vDef;
            }
        }

        [JsonIgnore]
        private VehicleDefinition vehicleDef;

        [JsonIgnore]
        public string localizedName => vehicleDef?.localizedName;

        [PublicAPI, JsonIgnore]
        public string invariantName => vehicleDef?.invariantName;

        [PublicAPI, JsonIgnore, Obsolete("Please be explicit and use localizedName or invariantName")]
        public string name => localizedName ?? string.Empty;

        public string loadoutDescription
        {
            get => descriptionDef?.edname;
            set
            {
                LoadoutDescription dDef = LoadoutDescription.FromEDName(value);
                descriptionDef = dDef;
            }
        }

        [PublicAPI, JsonIgnore]
        private LoadoutDescription descriptionDef;

        [JsonIgnore]
        public string localizedDescription => descriptionDef?.localizedName;

        [PublicAPI, JsonIgnore]
        [Obsolete("Please be explicit and use localizedDescription")]
        public string description => localizedDescription ?? string.Empty;

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
                vehicleDef = VehicleDefinition.FromEDName(edName),
                descriptionDef = LoadoutDescription.FromLoadoutName(loadoutName)
            };

            return vehicle;
        }

    }
}
