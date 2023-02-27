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

            None = new FederationRating("None", 0);
            Recruit = new FederationRating("Recruit", 1);
            Cadet = new FederationRating("Cadet", 2);
            Midshipman = new FederationRating("Midshipman", 3);
            PettyOfficer = new FederationRating("PettyOfficer", 4);
            ChiefPettyOfficer = new FederationRating("ChiefPettyOfficer", 5);
            WarrantOfficer = new FederationRating("WarrantOfficer", 6);
            Ensign = new FederationRating("Ensign", 7);
            Lieutenant = new FederationRating("Lieutenant", 8);
            LieutenantCommander = new FederationRating("LieutenantCommander", 9);
            PostCommander = new FederationRating("PostCommander", 10);
            PostCaptain = new FederationRating("PostCaptain", 11);
            RearAdmiral = new FederationRating("RearAdmiral", 12);
            ViceAdmiral = new FederationRating("ViceAdmiral", 13);
            Admiral = new FederationRating("Admiral", 14);
        }

        public static readonly FederationRating None;
        public static readonly FederationRating Recruit;
        public static readonly FederationRating Cadet;
        public static readonly FederationRating Midshipman;
        public static readonly FederationRating PettyOfficer;
        public static readonly FederationRating ChiefPettyOfficer;
        public static readonly FederationRating WarrantOfficer;
        public static readonly FederationRating Ensign;
        public static readonly FederationRating Lieutenant;
        public static readonly FederationRating LieutenantCommander;
        public static readonly FederationRating PostCommander;
        public static readonly FederationRating PostCaptain;
        public static readonly FederationRating RearAdmiral;
        public static readonly FederationRating ViceAdmiral;
        public static readonly FederationRating Admiral;
        
        [PublicAPI]
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
                Logging.Info("Unknown Federation Rating rank " + from);
            }
            return result;
        }
    }
}
