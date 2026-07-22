using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class NpcSuitLoadout : ResourceBasedLocalizedEDName<NpcSuitLoadout>
    {
        static NpcSuitLoadout()
        {
            resourceManager = Properties.NpcSuitLoadout.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        public static readonly NpcSuitLoadout CivilianAdmin = new("citizensuitai_admin"); // Administrator
        public static readonly NpcSuitLoadout CivilianScientist = new("citizensuitai_scientific"); // Researcher
        public static readonly NpcSuitLoadout CivilianWorker = new("citizensuitai_industrial"); // Technician
        public static readonly NpcSuitLoadout Commando = new("assaultsuitai"); // Commando
        public static readonly NpcSuitLoadout Scout = new("lightassaultsuitai"); // Scout
        public static readonly NpcSuitLoadout Sharpshooter = new("rangedsuitai"); // Sharpshooter
        public static readonly NpcSuitLoadout Striker = new("closesuitai"); // Striker
        public static readonly NpcSuitLoadout Pilot = new( "flightsuitai" ); // Pilot
        public static readonly NpcSuitLoadout TBD = new("heavysuitai"); // Commando

        [PublicAPI( "the grade of the suit loadout" )]
        public int grade { get; private set; }

        // dummy used to ensure that the static constructor has run
        public NpcSuitLoadout() : this("")
        { }

        private NpcSuitLoadout(string edname) : base(edname, edname)
        { }

        public new static NpcSuitLoadout FromEDName(string edname)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            var (tidiedName, grade) = titiedEDName(edname);
            var result = ResourceBasedLocalizedEDName<NpcSuitLoadout>.FromEDName(tidiedName);
            if (result != null) { result.grade = grade; }
            return result;
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, titiedEDName(edName).Item1, StringComparison.InvariantCultureIgnoreCase));
        }

        private static (string, int) titiedEDName(string edName)
        {
            var tidiedName = edName?.ToLowerInvariant().Replace("$", "").Replace(";", "").Replace("_name", "");
            if (int.TryParse(tidiedName?.Last().ToString(), out var grade))
            {
                tidiedName = tidiedName?.Replace("_class" + grade, "");
            }
            grade = grade == 0 ? 1 : grade; // Always at least grade 1
            return (tidiedName, grade);
        }
    }
}
