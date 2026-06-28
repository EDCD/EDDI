using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class VesselDefinition : ResourceBasedLocalizedEDName<VesselDefinition>
    {
        static VesselDefinition ()
        {
            resourceManager = Properties.Vehicle.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly VesselDefinition Fighter_Empire = new("Empire", VesselGroup.Telepresence); // Imperial GU-97 Fighter
        public static readonly VesselDefinition Fighter_Federation = new("Federation", VesselGroup.Telepresence); // Federal F63 Condor Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG7 = new("GdnHybridV1", VesselGroup.Telepresence); // XG7 Trident Guardian Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG8 = new("GdnHybridV2", VesselGroup.Telepresence); // XG8 Javalin Guardian Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG9 = new("GdnHybridV3", VesselGroup.Telepresence); // XG9 Lance Guardian Fighter
        public static readonly VesselDefinition Fighter_Independent = new("Independent", VesselGroup.Telepresence); // Independent Taipan Fighter
        public static readonly VesselDefinition SRV_Scarab = new("TestBuggy", VesselGroup.Piloted); // Scarab SRV
        public static readonly VesselDefinition SRV_Scorpion = new("CombatMulticrewSRV01", VesselGroup.Piloted); // Scorpion SRV
        public static readonly VesselDefinition Nomad = new( "Lander01", VesselGroup.Piloted ); // Nomad Exploration Vessel; basically a ship launched SRV

        public VesselGroup? vesselGroup { get; }

        // dummy used to ensure that the static constructor has run
        public VesselDefinition () : this( "", null )
        { }

        private VesselDefinition ( string edname, VesselGroup? vesselGroup ) : base( edname, edname )
        {
            this.vesselGroup = vesselGroup;
        }

        public static new VesselDefinition FromEDName ( string edName )
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

    public enum VesselGroup
    {
        Piloted,
        Telepresence
    }
}
