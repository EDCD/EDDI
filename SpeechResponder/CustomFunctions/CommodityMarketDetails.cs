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
        public string description => Properties.CustomFunctions_Untranslated.CommodityMarketDetails;
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
