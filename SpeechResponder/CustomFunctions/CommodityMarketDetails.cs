using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CommodityMarketDetails : ICustomFunction
    {
        public string name => "CommodityMarketDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a commodity, including information that is specific to a market, given the commodity name.

CommodityMarketDetails() takes one mandatory argument and two optional arguments. 
- The first argument, the name of the commodity for which you want more information, is mandatory.
- The second argument, the name of the station to reference for market data, is optional. If not given then EDDI will default to the current station (if the current station is not set and no station is specified then this shall return empty). 
- The third argument, the name of the system to reference for market data, is optional. If not given then EDDI will default to the current star system (if the specified station cannot be found within the current star system then EDDI shall return empty).

Common usage of this is to provide further information about a commodity, for example:

    {set marketcommodity to CommodityMarketDetails(""Pesticides"", ""Chelbin Service Station"", ""Wolf 397"")}
    {marketcommodity.name} is selling for {marketcommodity.sellprice} with a current market demand of {marketcommodity.demand} units.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            CommodityMarketQuote result = null;
            CommodityMarketQuote CommodityDetails(string commodityLocalizedName, Station station)
            {
                return station?.commodities?.FirstOrDefault(c => c.localizedName == commodityLocalizedName);
            }

            if (values.Count == 1)
            {
                // Named commodity, current station
                Station station = EDDI.Instance.CurrentStation;
                result = CommodityDetails(values[0].AsString, station);
            }
            else if (values.Count == 2)
            {
                // Named commodity, named station, current system 
                StarSystem system = EDDI.Instance.CurrentStarSystem;
                string stationName = values[1].AsString;
                Station station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                result = CommodityDetails(values[0].AsString, station);
            }
            else if (values.Count == 3)
            {
                // Named commodity, named station, named system 
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString);
                string stationName = values[1].AsString;
                Station station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                result = CommodityDetails(values[0].AsString, station);
            }
            return new ReflectionValue(result ?? new object());
        }, 0, 3);
    }
}
