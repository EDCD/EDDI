using System;
using System.Linq;

namespace EddiDataDefinitions
{
    /// <summary>
    /// NPC exclusive ships (e.g. Thargoid vessels)
    /// </summary>
    public class NpcShip : ResourceBasedLocalizedEDName<NpcShip>
    {
        static NpcShip()
        {
            resourceManager = Properties.NpcAuthorityShip.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly NpcShip ThargoidScout = new NpcShip("scout"); // Thargoid scout (or perhaps Thargon?)
        public static readonly NpcShip ThargoidScoutHQ = new NpcShip("scout_hq"); // Thargoid scout, variant HQ
        public static readonly NpcShip ThargoidScoutQ = new NpcShip("scout_q"); // Thargoid scout, variant Q
        public static readonly NpcShip ThargoidScoutNQ = new NpcShip("scout_nq"); // Thargoid scout, variant NQ

        public static readonly NpcShip ThargoidCyclops = new NpcShip("unknownsaucer"); // Thargoid Cyclops
        public static readonly NpcShip ThargoidBasilisk = new NpcShip("unknownsaucer_e"); // Thargoid Basilisk
        public static readonly NpcShip ThargoidMedusa = new NpcShip("unknownsaucer_f"); // Thargoid Medusa
        public static readonly NpcShip ThargoidOrthrus = new NpcShip("unknownsaucer_g"); // Thargoid Orthrus
        public static readonly NpcShip ThargoidHydra = new NpcShip("unknownsaucer_h"); // Thargoid Hydra

        // dummy used to ensure that the static constructor has run
        public NpcShip() : this("")
        { }

        private NpcShip(string edname) : base(edname, edname)
        { }

        public new static NpcShip FromEDName(string edname)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            var tidiedName = titiedEDName(edname);
            var result = ResourceBasedLocalizedEDName<NpcShip>.FromEDName(tidiedName);
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
                .Replace(";", "");
            return tidiedName;
        }
    }
}
