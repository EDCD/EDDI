using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public int subslot { get; private set; }

        [PublicAPI("the short name of the vehicle's loadout")]
        public string loadout { get; private set; }

        [PublicAPI("the number of times the vehicle may be rebuilt")]
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

        [PublicAPI( "true if the vehicle is remotely piloted via telepresence" )]
        public bool isRemotePiloted => vehicleDef?.vesselGroup == VesselGroup.Telepresence;

        [JsonIgnore]
        public string localizedName => vehicleDef?.localizedName;

        [PublicAPI( "the invariant name of the vehicle, for example 'F63 Condor'" ), JsonIgnore]
        public string invariantName => vehicleDef?.invariantName;

        [PublicAPI( "the localized name of the vehicle, for example 'F63 Condor'" ), JsonIgnore, Obsolete("Please be explicit and use localizedName or invariantName")]
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

        [PublicAPI("the description of the vehicle's loadout, as an object"), JsonIgnore]
        private LoadoutDescription descriptionDef;

        [JsonIgnore]
        public string localizedDescription => descriptionDef?.localizedName;

        [PublicAPI( "the invariant description of the vehicle's loadout" ), JsonIgnore]
        public string invariantDescription => descriptionDef?.invariantName;

        [PublicAPI( "the localized description of the vehicle's loadout" ), JsonIgnore]
        [Obsolete("Please be explicit and use localizedDescription")]
        public string description => localizedDescription ?? string.Empty;

        public static Vehicle FromJson(int subslot, dynamic json)
        {
            if (json is null) { return null; }

            var edName = (string)json["name"];
            var loadout = (string)json["loadout"];

            var vehicle = new Vehicle()
            {
                loadout = loadout,
                rebuilds = (int)json["rebuilds"],
                subslot = subslot,
                vehicleDef = VesselDefinition.FromEDName(edName),
                descriptionDef = LoadoutDescription.FromVesselAndLoadoutEDName(edName, loadout)
            };

            return vehicle;
        }

    }
}
