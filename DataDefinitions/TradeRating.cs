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

            var Penniless = new TradeRating("Penniless", 0);
            var MostlyPenniless = new TradeRating("MostlyPenniless", 1);
            var Peddler = new TradeRating("Peddler", 2);
            var Dealer = new TradeRating("Dealer", 3);
            var Merchant = new TradeRating("Merchant", 4);
            var Broker = new TradeRating("Broker", 5);
            var Entrepreneur = new TradeRating("Entrepreneur", 6);
            var Tycoon = new TradeRating("Tycoon", 7);
            var Elite = new TradeRating("Elite", 8);
        }

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
                Logging.Report("Unknown Trade Rating rank " + from);
            }
            return result;
        }
    }
}
