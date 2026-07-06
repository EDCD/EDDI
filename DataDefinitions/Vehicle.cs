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
                var vDef = VesselDefinition.FromEDName(value);
                vehicleDef = vDef;
            }
        }

        [JsonIgnore]
        private VesselDefinition vehicleDef;

        [PublicAPI]
        public bool isRemotePiloted => vehicleDef?.vesselGroup == VesselGroup.Telepresence;

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
                var dDef = LoadoutDescription.FromEDName(value);
                descriptionDef = dDef;
            }
        }

        [PublicAPI, JsonIgnore]
        private LoadoutDescription descriptionDef;

        [JsonIgnore]
        public string localizedDescription => descriptionDef?.localizedName;

        [PublicAPI, JsonIgnore]
        public string invariantDescription => descriptionDef?.invariantName;

        [PublicAPI, JsonIgnore]
        [Obsolete("Please be explicit and use localizedDescription")]
        public string description => localizedDescription ?? string.Empty;

        public static Vehicle FromJson(int subslot, dynamic json)
        {
            if (json is null) { return null; }

            var edName = (string)json["name"];
            var loadoutName = (string)json["loadoutName"];

            var vehicle = new Vehicle()
            {
                loadout = (string)json["loadout"],
                rebuilds = (int)json["rebuilds"],
                subslot = subslot,
                vehicleDef = VesselDefinition.FromEDName(edName),
                descriptionDef = LoadoutDescription.FromLoadoutName(loadoutName)
            };

            return vehicle;
        }

    }
}
