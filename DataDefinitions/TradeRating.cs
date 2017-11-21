using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Trade ratings
    /// </summary>
    public class TradeRating
    {
        private static readonly List<TradeRating> RATINGS = new List<TradeRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public int rank { get; private set; }

        private TradeRating(string edname, int rank, string name)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;

            RATINGS.Add(this);
        }

        public static readonly TradeRating Penniless = new TradeRating("Penniless", 0, "Penniless");
        public static readonly TradeRating MostlyPenniless = new TradeRating("MostlyPenniless", 1, "Mostly Penniless");
        public static readonly TradeRating Peddler = new TradeRating("Peddler", 2, "Peddler");
        public static readonly TradeRating Dealer = new TradeRating("Dealer", 3, "Dealer");
        public static readonly TradeRating Merchant = new TradeRating("Merchant", 4, "Merchant");
        public static readonly TradeRating Broker = new TradeRating("Broker", 5, "Broker");
        public static readonly TradeRating Entrepreneur = new TradeRating("Entrepreneur", 6, "Entrepreneur");
        public static readonly TradeRating Tycoon = new TradeRating("Tycoon", 7, "Tycoon");
        public static readonly TradeRating Elite = new TradeRating("Elite", 8, "Elite");

        public static TradeRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            TradeRating result = RATINGS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Trade Rating name " + from);
            }
            return result;
        }

        public static TradeRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            TradeRating result = RATINGS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Trade Rating ED name " + from);
            }
            return result;
        }

        public static TradeRating FromRank(int from)
        {
            TradeRating result = RATINGS.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Trade Rating rank " + from);
            }
            return result;
        }
    }
}
