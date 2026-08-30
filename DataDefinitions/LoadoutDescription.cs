using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class LoadoutDescription : ResourceBasedLocalizedEDName<LoadoutDescription>
    {
        static LoadoutDescription ()
        {
            resourceManager = Properties.LoadoutDescription.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly LoadoutDescription EmpireZero = new("Empire_Fighter", "Zero", "EmpireZero");
        public static readonly LoadoutDescription EmpireOne = new("Empire_Fighter", "One", "EmpireZero");
        public static readonly LoadoutDescription EmpireTwo = new("Empire_Fighter", "Two", "EmpireZero");
        public static readonly LoadoutDescription EmpireThree = new("Empire_Fighter", "Three", "EmpireThree");
        public static readonly LoadoutDescription EmpireFour = new("Empire_Fighter", "Four", "EmpireFour");
        public static readonly LoadoutDescription FederationZero = new("Federation_Fighter", "Zero", "FederationZero");
        public static readonly LoadoutDescription FederationOne = new("Federation_Fighter", "One", "FederationZero");
        public static readonly LoadoutDescription FederationTwo = new("Federation_Fighter", "Two", "FederationZero");
        public static readonly LoadoutDescription FederationThree = new("Federation_Fighter", "Three", "FederationThree");
        public static readonly LoadoutDescription FederationFour = new("Federation_Fighter", "Four", "FederationFour");
        public static readonly LoadoutDescription GdnHybridV1 = new("Gdn_Hybrid_Fighter_V1", "GdnHybridV1", "GdnHybridV1");
        public static readonly LoadoutDescription GdnHybridV2 = new("Gdn_Hybrid_Fighter_V2", "GdnHybridV2", "GdnHybridV2");
        public static readonly LoadoutDescription GdnHybridV3 = new("Gdn_Hybrid_Fighter_V3", "GdnHybridV3", "GdnHybridV3");
        public static readonly LoadoutDescription IndependentZero = new("Independent_Fighter", "Zero", "IndependentZero");
        public static readonly LoadoutDescription IndependentOne = new("Independent_Fighter", "One", "IndependentOne");
        public static readonly LoadoutDescription IndependentTwo = new("Independent_Fighter", "Two", "IndependentTwo");
        public static readonly LoadoutDescription IndependentThree = new("Independent_Fighter", "Three", "IndependentThree");
        public static readonly LoadoutDescription IndependentFour = new("Independent_Fighter", "Four", "IndependentFour");
        public static readonly LoadoutDescription IndependentAT = new("Independent_Fighter", "AT", "IndependentAT");
        public static readonly LoadoutDescription Starter = new("TestBuggy", "Starter", "Starter"); // Scarab SRV
        public static readonly LoadoutDescription Default = new("Combat_Multicrew_SRV_01", "Default", "Default"); // Scorpion SRV
        public static readonly LoadoutDescription NomadBase = new("Lander01", "Base", "NomadBase"); // Nomad Standard Edition
        public static readonly LoadoutDescription NomadAdvanced = new("Lander01", "Advanced", "NomadAdvanced"); // Nomad Stellar Edition
        public static readonly LoadoutDescription NomadGalactic = new("Lander01", "Galactic", "NomadGalactic"); // Nomad Galactic Edition
        public static readonly LoadoutDescription RhinoBase = new("MEV_Rhino", "Base", "RhinoBase"); // Rhino Standard Edition
        public static readonly LoadoutDescription RhinoAdvanced = new("MEV_Rhino", "Advanced", "RhinoAdvanced"); // Rhino Stellar Edition
        public static readonly LoadoutDescription RhinoGalactic = new("MEV_Rhino", "Galactic", "RhinoGalactic"); // Rhino Galactic Edition
        public static readonly LoadoutDescription RhinoMiningAdvanced = new("MEV_Rhino", "MiningAdvanced", "RhinoMiningAdvanced"); // Rhino Stellar Mining Edition
        public static readonly LoadoutDescription RhinoMiningGalactic = new("MEV_Rhino", "MiningGalactic", "RhinoMiningGalactic"); // Rhino Galactic Mining Edition

        public readonly string vesselEDName;

        // dummy used to ensure that the static constructor has run
        public LoadoutDescription () : this( "", "", "" )
        { }

        private LoadoutDescription ( string vesselEDName, string edname, string basename ) : base( edname, basename )
        { 
            this.vesselEDName = vesselEDName;
        }

        public static LoadoutDescription FromVesselAndLoadoutEDName ( string vesselEDName, string loadoutEDName )
        {
            if ( loadoutEDName == null )
            {
                return null;
            }

            var tidiedLoadout = loadoutEDName.ToLowerInvariant()
                .Replace("_", "")
                .Replace("fighterloadout", "")
                .Replace("name", "");
            return AllOfThem
                .Where(v => v.vesselEDName.Equals(vesselEDName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault( v => string.Equals( v.edname, tidiedLoadout, StringComparison.OrdinalIgnoreCase ) );
        }
    }
}
