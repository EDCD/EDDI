using System.Collections.Generic;
using Utilities;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class Commodity
    {
        // Definition of the commodity
        public string name { get; set; }
        public string category { get; set; }
        public int? avgprice { get; set; }
        public bool? rare { get; set; }

        // Per-station information
        public int? buyprice { get; set; }
        public int? stock { get; set; }
        // StockBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public dynamic stockbracket { get; set; }
        public int? sellprice { get; set; }
        public int? demand { get; set; }
        // DemandBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public dynamic demandbracket { get; set; }
        public List<string> StatusFlags { get; set; }

        // Admin
        public long EDDBID { get; set; }
        public string EDName { get; set; }

        [JsonIgnore]
        public string LocalName
        {
            get
            {
                return I18N.GetString(EDName) ?? EDName;
            }
        }

        public Commodity() { }

        public Commodity(Commodity Commodity)
        {
            this.EDDBID = Commodity.EDDBID;
            this.EDName = Commodity.EDName;
            this.name = Commodity.name;
            this.category = Commodity.category;
            this.avgprice = Commodity.avgprice;
            this.rare = Commodity.rare;
        }

        public Commodity(long EDDBID, string EDName, string Name, string Category, int AveragePrice, bool Rare)
        {
            this.EDDBID = EDDBID;
            this.EDName = EDName;
            this.name = Name;
            this.category = Category;
            this.avgprice = AveragePrice;
            this.rare = Rare;
        }

        // Constructor when commodity's actual name matches its ED name
        public Commodity(long EDDBID, string Name, string Category, int AveragePrice, bool Rare) : this(EDDBID, Name, Name, Category, AveragePrice, Rare)
        {
        }

        // Constructor from EDDB information
        public Commodity(long EDDBID, string Name, int? buyprice, int? demand, int? sellprice, int? supply)
        {
            this.EDDBID = EDDBID;
            this.name = Name;
            this.buyprice = buyprice;
            this.demand = demand;
            this.sellprice = sellprice;
            this.stock = supply;
        }
    }
}
