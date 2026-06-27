using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class VehicleDefinition : ResourceBasedLocalizedEDName<VehicleDefinition>
    {
        static VehicleDefinition ()
        {
            resourceManager = Properties.Vehicle.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly VehicleDefinition Fighter_Empire = new("Empire"); // Imperial GU-97 Fighter
        public static readonly VehicleDefinition Fighter_Federation = new("Federation"); // Federal F63 Condor Fighter
        public static readonly VehicleDefinition Fighter_Gdn_XG7 = new("GdnHybridV1"); // XG7 Trident Guardian Fighter
        public static readonly VehicleDefinition Fighter_Gdn_XG8 = new("GdnHybridV2"); // XG8 Javalin Guardian Fighter
        public static readonly VehicleDefinition Fighter_Gdn_XG9 = new("GdnHybridV3"); // XG9 Lance Guardian Fighter
        public static readonly VehicleDefinition Fighter_Independent = new("Independent"); // Independent Taipan Fighter
        public static readonly VehicleDefinition SRV_Scarab = new("TestBuggy"); // Scarab SRV
        public static readonly VehicleDefinition SRV_Scorpion = new("CombatMulticrewSRV01"); // Scorpion SRV
        public static readonly VehicleDefinition Nomad = new( "Lander01" ); // Nomad Exploration Vessel;

        // dummy used to ensure that the static constructor has run
        public VehicleDefinition () : this( "" )
        { }

        private VehicleDefinition ( string edname ) : base( edname, edname )
        { }

        public static new VehicleDefinition FromEDName ( string edName )
        {
            if ( edName == null )
            { return null; }

            return AllOfThem.FirstOrDefault( v =>
                string.Equals( v.edname, tidiedEDName( edName ), StringComparison.OrdinalIgnoreCase ) );
        }

        public static bool EDNameExists ( string edName )
        {
            if ( edName == null )
            { return false; }
            return AllOfThem.Any( v => string.Equals( v.edname, tidiedEDName( edName ), StringComparison.OrdinalIgnoreCase ) );
        }

        private static string tidiedEDName ( string edName )
        {
            return edName?.ToLowerInvariant().Replace( "_fighter", "" ).Replace( "_", "" );
        }
    }
}
