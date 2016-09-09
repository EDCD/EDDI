using System.Collections.Generic;

namespace EliteDangerousDataDefinitions
{
    public class Commodity
    {
        // Definition of the commodity
        public string Name { get; set; }
        public string Category { get; set; }
        public int AveragePrice { get; set; }
        public bool Rare { get; set; }

        // Per-station information
        public int BuyPrice { get; set; }
        public int Stock { get; set; }
        // StockBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public dynamic StockBracket { get; set; }
        public int SellPrice { get; set; }
        public int Demand { get; set; }
        // DemandBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public dynamic DemandBracket { get; set; }
        public List<string> StatusFlags { get; set; }

        // Admin
        public long EDDBID { get; set; }
        public string EDName { get; set; }

        public Commodity() { }

        public Commodity(Commodity Commodity)
        {
            this.EDDBID = Commodity.EDDBID;
            this.EDName = Commodity.EDName;
            this.Name = Commodity.Name;
            this.Category = Commodity.Category;
            this.AveragePrice = Commodity.AveragePrice;
            this.Rare = Commodity.Rare;
        }

        public Commodity(long EDDBID, string EDName, string Name, string Category, int AveragePrice, bool Rare)
        {
            this.EDDBID = EDDBID;
            this.EDName = EDName;
            this.Name = Name;
            this.Category = Category;
            this.AveragePrice = AveragePrice;
            this.Rare = Rare;
        }

        // Constructor when commodity's actual name matches its ED name
        public Commodity(long EDDBID, string Name, string Category, int AveragePrice, bool Rare) : this(EDDBID, Name, Name, Category, AveragePrice, Rare)
        {
        }
    }
}
