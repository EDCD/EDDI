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

        public static readonly LoadoutDescription EmpireZero = new("EmpireZero", VesselDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireOne = new("EmpireOne", VesselDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireTwo = new("EmpireTwo", VesselDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireThree = new("EmpireThree", VesselDefinition.Fighter_Empire);
        public static readonly LoadoutDescription EmpireFour = new("EmpireFour", VesselDefinition.Fighter_Empire);
        public static readonly LoadoutDescription FederationZero = new("FederationZero", VesselDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationOne = new("FederationOne", VesselDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationTwo = new("FederationTwo", VesselDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationThree = new("FederationThree", VesselDefinition.Fighter_Federation);
        public static readonly LoadoutDescription FederationFour = new("FederationFour", VesselDefinition.Fighter_Federation);
        public static readonly LoadoutDescription GdnHybridV1 = new("GdnHybridV1", VesselDefinition.Fighter_Gdn_XG7);
        public static readonly LoadoutDescription GdnHybridV2 = new("GdnHybridV2", VesselDefinition.Fighter_Gdn_XG8);
        public static readonly LoadoutDescription GdnHybridV3 = new("GdnHybridV3", VesselDefinition.Fighter_Gdn_XG9);
        public static readonly LoadoutDescription IndependentZero = new("IndependentZero", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentOne = new("IndependentOne", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentTwo = new("IndependentTwo", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentThree = new("IndependentThree", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentFour = new("IndependentFour", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription IndependentAT = new("IndependentAT", VesselDefinition.Fighter_Independent);
        public static readonly LoadoutDescription Starter = new("Starter", VesselDefinition.SRV_Scarab);
        public static readonly LoadoutDescription Default = new("Default", VesselDefinition.SRV_Scorpion);
        public static readonly LoadoutDescription Base = new("Base", VesselDefinition.Nomad);

        public readonly VesselDefinition vehicle;

        // dummy used to ensure that the static constructor has run
        public LoadoutDescription () : this( "", null )
        { }

        private LoadoutDescription ( string edname, VesselDefinition vehicle ) : base( edname, edname )
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
