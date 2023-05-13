using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class NpcAuthorityShip : ResourceBasedLocalizedEDName<NpcAuthorityShip>
    {
        static NpcAuthorityShip()
        {
            resourceManager = Properties.NpcAuthorityShip.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly NpcAuthorityShip MilitaryAlliance = new NpcAuthorityShip("Military_Alliance");
        public static readonly NpcAuthorityShip MilitaryEmpire = new NpcAuthorityShip("Military_Empire");
        public static readonly NpcAuthorityShip MilitaryFederation = new NpcAuthorityShip("Military_Federation");
        public static readonly NpcAuthorityShip MilitaryIndependent = new NpcAuthorityShip("Military_Independent");

        public static readonly NpcAuthorityShip PoliceAlliance = new NpcAuthorityShip("Police_Alliance");
        public static readonly NpcAuthorityShip PoliceEmpire = new NpcAuthorityShip("Police_Empire");
        public static readonly NpcAuthorityShip PoliceFederation = new NpcAuthorityShip("Police_Federation");
        public static readonly NpcAuthorityShip PoliceIndependent = new NpcAuthorityShip("Police_Independent");

        public static readonly NpcAuthorityShip Thargoid = new NpcAuthorityShip( "Thargoid" );
        public static readonly NpcAuthorityShip UNKNOWN = new NpcAuthorityShip("UNKNOWN"); // Thargoid?

        // dummy used to ensure that the static constructor has run
        public NpcAuthorityShip() : this("")
        { }

        private NpcAuthorityShip(string edname) : base(edname, edname)
        { }

        public new static NpcAuthorityShip FromEDName(string edname)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            var tidiedName = titiedEDName(edname);
            var result = ResourceBasedLocalizedEDName<NpcAuthorityShip>.FromEDName(tidiedName);
            return result;
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, titiedEDName(edName), StringComparison.InvariantCultureIgnoreCase));
        }

        private static string titiedEDName(string edName)
        {
            var tidiedName = edName?.ToLowerInvariant()
                .Replace("$", "")
                .Replace("shipname_", "")
                .Replace(";", "");
            return tidiedName;
        }
    }
}
