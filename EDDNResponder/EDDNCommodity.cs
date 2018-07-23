using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EDDNResponder
{
    public class EDDNCommodity
    {
        // Schema reference: https://github.com/EDSM-NET/EDDN/blob/master/schemas/commodity-v3.0.json
        public string name;
        public int meanPrice;
        public int buyPrice;
        public int stock;
        public dynamic stockBracket; // Possible values are 0, 1, 2, 3, or ""
        public int sellPrice;
        public int demand;
        public dynamic demandBracket; // Possible values are 0, 1, 2, 3, or ""
        public List<string> statusFlags = new List<string>();

        public bool ShouldSerializestatusFlags()
        {
            // Don't serialize status flags if they are empty as the schema requires that if present they contain at least 1 element
            return (statusFlags != null && statusFlags.Count > 0);
        }

        public EDDNCommodity(CommodityMarketQuote quote)
        {
            name = quote.definition.edname;
            meanPrice = quote.definition.avgprice;
            buyPrice = quote.buyprice;
            stock = quote.stock;
            stockBracket = quote.stockbracket;
            sellPrice = quote.sellprice;
            demand = quote.demand;
            demandBracket = quote.demandbracket;
            if (quote.StatusFlags.Count > 0)
            {
                statusFlags = quote.StatusFlags;
            }
        }
    }
}
