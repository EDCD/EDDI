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

        public static readonly VesselDefinition Fighter_Empire = new("Empire_Fighter", VesselGroup.Telepresence); // Imperial GU-97 Fighter
        public static readonly VesselDefinition Fighter_Federation = new("Federation_Fighter", VesselGroup.Telepresence); // Federal F63 Condor Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG7 = new("Gdn_Hybrid_Fighter_V1", VesselGroup.Telepresence); // XG7 Trident Guardian Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG8 = new("Gdn_Hybrid_Fighter_V2", VesselGroup.Telepresence); // XG8 Javalin Guardian Fighter
        public static readonly VesselDefinition Fighter_Gdn_XG9 = new("Gdn_Hybrid_Fighter_V3", VesselGroup.Telepresence); // XG9 Lance Guardian Fighter
        public static readonly VesselDefinition Fighter_Independent = new("Independent_Fighter", VesselGroup.Telepresence); // Independent Taipan Fighter
        public static readonly VesselDefinition SRV_Scarab = new("TestBuggy", VesselGroup.Piloted); // Scarab SRV
        public static readonly VesselDefinition SRV_Scorpion = new("Combat_Multicrew_SRV_01", VesselGroup.Piloted); // Scorpion SRV
        public static readonly VesselDefinition Nomad = new( "Lander01", VesselGroup.Piloted ); // Nomad Exploration Vessel; basically a ship launched SRV

        public VesselGroup? vesselGroup { get; }

        // dummy used to ensure that the static constructor has run
        public VesselDefinition () : this( "", null )
        { }

        private VesselDefinition ( string edname, VesselGroup? vesselGroup ) : base( edname, edname.ToLowerInvariant().Replace( "_fighter", "" ).Replace( "_", "" ) )
        {
            this.vesselGroup = vesselGroup;
        }

        public static new VesselDefinition FromEDName ( string edName )
        {
            if ( edName == null ) { return null; }

            return AllOfThem.FirstOrDefault( v =>
                string.Equals( v.edname, edName, StringComparison.OrdinalIgnoreCase ) );
        }

        public static bool inFighter ( string edName )
        {
            if ( string.IsNullOrEmpty( edName ) ) { return false; }

            return edName.Contains( "Fighter", StringComparison.OrdinalIgnoreCase );
        }

        public static bool inSRV ( string edName )
        {
            if ( string.IsNullOrEmpty( edName ) ) { return false; }

            return ( !edName.Contains( "Fighter", StringComparison.OrdinalIgnoreCase ) && 
                   edName.Contains( "SRV", StringComparison.OrdinalIgnoreCase )) ||
                   edName.Contains( "Buggy", StringComparison.OrdinalIgnoreCase ) ||
                   edName.Contains( "Lander", StringComparison.OrdinalIgnoreCase );
        }

        public static bool isVessel ( string edName )
        {
            if ( string.IsNullOrEmpty( edName ) ) { return false; }

            return inFighter( edName ) || inSRV( edName ) ||
                   AllOfThem.Any( v => v.edname.Contains( edName, StringComparison.OrdinalIgnoreCase ) );
        }
    }

    public enum VesselGroup
    {
        Piloted,
        Telepresence
    }
}
