using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Federation ratings
    /// </summary>
    public class FederationRating : ResourceBasedLocalizedEDName<FederationRating>
    {
        static FederationRating()
        {
            resourceManager = Properties.FederationRatings.ResourceManager;
            resourceManager.IgnoreCase = false;

            var None = new FederationRating("None", 0);
            var Recruit = new FederationRating("Recruit", 1);
            var Cadet = new FederationRating("Cadet", 2);
            var Midshipman = new FederationRating("Midshipman", 3);
            var PettyOfficer = new FederationRating("PettyOfficer", 4);
            var ChiefPettyOfficer = new FederationRating("ChiefPettyOfficer", 5);
            var WarrantOfficer = new FederationRating("WarrantOfficer", 6);
            var Ensign = new FederationRating("Ensign", 7);
            var Lieutenant = new FederationRating("Lieutenant", 8);
            var LieutenantCommander = new FederationRating("LieutenantCommander", 9);
            var PostCommander = new FederationRating("PostCommander", 10);
            var PostCaptain = new FederationRating("PostCaptain", 11);
            var RearAdmiral = new FederationRating("RearAdmiral", 12);
            var ViceAdmiral = new FederationRating("ViceAdmiral", 13);
            var Admiral = new FederationRating("Admiral", 14);
        }
        
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public FederationRating() : this("", 0)
        { }

        private FederationRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static FederationRating FromRank(int from)
        {
            FederationRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Federation Rating rank " + from);
            }
            return result;
        }
    }
}
