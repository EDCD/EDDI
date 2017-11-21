using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Federation ratings
    /// </summary>
    public class FederationRating
    {
        private static readonly List<FederationRating> RATINGS = new List<FederationRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public int rank { get; private set; }

        private FederationRating(string edname, int rank, string name)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;

            RATINGS.Add(this);
        }

        public static readonly FederationRating None = new FederationRating("None", 0, "None");
        public static readonly FederationRating Recruit = new FederationRating("Recruit", 1, "Recruit");
        public static readonly FederationRating Cadet = new FederationRating("Cadet", 2, "Cadet");
        public static readonly FederationRating Midshipman = new FederationRating("Midshipman", 3, "Midshipman");
        public static readonly FederationRating PettyOfficer = new FederationRating("PettyOfficer", 4, "Petty Officer");
        public static readonly FederationRating ChiefPettyOfficer = new FederationRating("ChiefPettyOfficer", 5, "Chief Petty Officer");
        public static readonly FederationRating WarrantOfficer = new FederationRating("WarrantOfficer", 6, "Warrant Officer");
        public static readonly FederationRating Ensign = new FederationRating("Ensign", 7, "Ensign");
        public static readonly FederationRating Lieutenant = new FederationRating("Lieutenant", 8, "Lieutenant");
        public static readonly FederationRating LieutenantCommander = new FederationRating("LieutenantCommander", 9, "Lieutenant Commander");
        public static readonly FederationRating PostCommander = new FederationRating("PostCommander", 10, "Post Commander");
        public static readonly FederationRating PostCaptain = new FederationRating("PostCaptain", 11, "Post Captain");
        public static readonly FederationRating RearAdmiral = new FederationRating("RearAdmiral", 12, "Rear Admiral");
        public static readonly FederationRating ViceAdmiral = new FederationRating("ViceAdmiral", 13, "Vice Admiral");
        public static readonly FederationRating Admiral = new FederationRating("Admiral", 14, "Admiral");

        public static FederationRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            FederationRating result = RATINGS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Federation Rating name " + from);
            }
            return result;
        }

        public static FederationRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            FederationRating result = RATINGS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Federation Rating ED name " + from);
            }
            return result;
        }

        public static FederationRating FromRank(int from)
        {
            FederationRating result = RATINGS.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Federation Rating rank " + from);
            }
            return result;
        }
    }
}
