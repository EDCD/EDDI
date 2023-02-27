using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Trade ratings
    /// </summary>
    public class TradeRating : ResourceBasedLocalizedEDName<TradeRating>
    {
        static TradeRating()
        {
            resourceManager = Properties.TradeRatings.ResourceManager;
            resourceManager.IgnoreCase = false;

            Penniless = new TradeRating("Penniless", 0);
            MostlyPenniless = new TradeRating("MostlyPenniless", 1);
            Peddler = new TradeRating("Peddler", 2);
            Dealer = new TradeRating("Dealer", 3);
            Merchant = new TradeRating("Merchant", 4);
            Broker = new TradeRating("Broker", 5);
            Entrepreneur = new TradeRating("Entrepreneur", 6);
            Tycoon = new TradeRating("Tycoon", 7);
            Elite = new TradeRating("Elite", 8);
            EliteI = new TradeRating("EliteI", 9);
            EliteII = new TradeRating("EliteII", 10);
            EliteIII = new TradeRating("EliteIII", 11);
            EliteIV = new TradeRating("EliteIV", 12);
            EliteV = new TradeRating("EliteV", 13);
        }

        public static readonly TradeRating Penniless;
        public static readonly TradeRating MostlyPenniless;
        public static readonly TradeRating Peddler;
        public static readonly TradeRating Dealer;
        public static readonly TradeRating Merchant;
        public static readonly TradeRating Broker;
        public static readonly TradeRating Entrepreneur;
        public static readonly TradeRating Tycoon;
        public static readonly TradeRating Elite;
        public static readonly TradeRating EliteI;
        public static readonly TradeRating EliteII;
        public static readonly TradeRating EliteIII;
        public static readonly TradeRating EliteIV;
        public static readonly TradeRating EliteV;

        [PublicAPI]
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public TradeRating() : this("", 0)
        { }

        private TradeRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static TradeRating FromRank(int from)
        {
            TradeRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Trade Rating rank " + from);
            }
            return result;
        }
    }
}
