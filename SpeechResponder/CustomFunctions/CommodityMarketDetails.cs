using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CommodityMarketDetails : ICustomFunction
    {
        public string name => "CommodityMarketDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CommodityMarketDetails;
        public Type ReturnType => typeof( CommodityMarketQuote );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            CommodityMarketQuote result = null;
            CommodityMarketQuote CommodityDetails(string commodityLocalizedName, Station station)
            {
                return station?.commodities?.FirstOrDefault(c => c.localizedName == commodityLocalizedName) ??
                       new CommodityMarketQuote(CommodityDefinition.FromNameOrEDName(values[0].AsString));
            }

            if (values.Count == 1)
            {
                // Named commodity, current station
                var station = EDDI.Instance.CurrentStation;
                result = CommodityDetails(values[0].AsString, station);
            }
            else if (values.Count == 2)
            {
                // Named commodity, named station, current system 
                var system = EDDI.Instance.CurrentStarSystem;
                var stationName = values[1].AsString;
                var station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                result = CommodityDetails(values[0].AsString, station);
            }
            else if (values.Count == 3)
            {
                // Named commodity, named station, named system 
                var system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString);
                var stationName = values[1].AsString;
                var station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                result = CommodityDetails(values[0].AsString, station);
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 0, 3);
    }
}
