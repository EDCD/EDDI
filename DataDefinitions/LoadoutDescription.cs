using System;
using System.Linq;

namespace EddiDataDefinitions
{
    class LoadoutDescription : ResourceBasedLocalizedEDName<LoadoutDescription>
    {
        static LoadoutDescription ()
        {
            resourceManager = Properties.LoadoutDescription.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly LoadoutDescription EmpireZero = new("EmpireZero", VehicleDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireOne = new("EmpireOne", VehicleDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireTwo = new("EmpireTwo", VehicleDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireThree = new("EmpireThree", VehicleDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireFour = new("EmpireFour", VehicleDefinition.Fighter_Empire);
        public static readonly LoadoutDescription FederationZero = new("FederationZero", VehicleDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationOne = new("FederationOne", VehicleDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationTwo = new("FederationTwo", VehicleDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationThree = new("FederationThree", VehicleDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationFour = new("FederationFour", VehicleDefinition.Fighter_Federation);
        public static readonly LoadoutDescription GdnHybridV1 = new("GdnHybridV1", VehicleDefinition.Fighter_Gdn_XG7);
        public static readonly LoadoutDescription GdnHybridV2 = new("GdnHybridV2", VehicleDefinition.Fighter_Gdn_XG8);
        public static readonly LoadoutDescription GdnHybridV3 = new("GdnHybridV3", VehicleDefinition.Fighter_Gdn_XG9);
        public static readonly LoadoutDescription IndependentZero = new("IndependentZero", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentOne = new("IndependentOne", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentTwo = new("IndependentTwo", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentThree = new("IndependentThree", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentFour = new("IndependentFour", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentAT = new("IndependentAT", VehicleDefinition.Fighter_Independent);
        public static readonly LoadoutDescription Starter = new("Starter", VehicleDefinition.SRV_Scarab);
        public static readonly LoadoutDescription Default = new("Default", VehicleDefinition.SRV_Scorpion);
        public static readonly LoadoutDescription Base = new("Base", VehicleDefinition.Nomad);

        public readonly VehicleDefinition vehicle;

        // dummy used to ensure that the static constructor has run
        public LoadoutDescription () : this( "", null )
        { }

        private LoadoutDescription ( string edname, VehicleDefinition vehicle ) : base( edname, edname )
        {
            this.vehicle = vehicle;
        }

        public static LoadoutDescription FromLoadoutName ( string loadoutName )
        {
            if ( loadoutName == null )
            {
                return null;
            }

            var tidiedLoadout = loadoutName.ToLowerInvariant()
                .Replace("_", "")
                .Replace("fighterloadout", "")
                .Replace("name", "");
            return AllOfThem.FirstOrDefault( v =>
                string.Equals( v.edname, tidiedLoadout, StringComparison.OrdinalIgnoreCase ) );
        }
    }
}
