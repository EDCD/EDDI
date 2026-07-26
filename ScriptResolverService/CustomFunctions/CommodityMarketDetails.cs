using Cottle;
using EddiCore;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities;

namespace EddiScriptResolverService.CustomFunctions
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
            try
            {
                // Fetch the commodity market details with a timeout
                var result = GetCommodityMarketDetails(values, TimeSpan.FromSeconds(10));
                return result is null
                    ? Value.EmptyMap
                    : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            catch ( TimeoutException ex )
            {
                // Log timeout exceptions
                Logging.Warn( $"Network request for {values[ 0 ].AsString} commodity market details timed out.", ex );
                return Value.EmptyMap;
            }
            catch ( Exception ex )
            {
                // Log unexpected exceptions
                Logging.Error( $"Error while fetching commodity market details for station {values[ 0 ].AsString}.", ex );
                return Value.EmptyMap;
            }
        }, 0, 3);

        private static CommodityMarketQuote GetCommodityMarketDetails ( IReadOnlyList<Value> values, TimeSpan timeout )
        {
            if ( values.Count == 1 )
            {
                // Named commodity, current station
                var station = EDDI.Instance.GameState.CurrentStation;
                return CommodityDetails( values[ 0 ].AsString, station );
            }

            if ( values.Count == 2 )
            {
                // Named commodity, named station, current system
                var system = EDDI.Instance.GameState.CurrentStarSystem;
                var stationName = values[1].AsString;
                var station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                return CommodityDetails( values[ 0 ].AsString, station );
            }

            if ( values.Count == 3 )
            {
                // Named commodity, named station, named system
                var system = GetStarSystem(values[2].AsString, timeout);
                var stationName = values[1].AsString;
                var station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                return CommodityDetails( values[ 0 ].AsString, station );
            }

            return null;

            CommodityMarketQuote CommodityDetails ( string commodityLocalizedName, Station station )
            {
                return station?.commodities.FirstOrDefault( c => c.localizedName == commodityLocalizedName ) ??
                       new CommodityMarketQuote( CommodityDefinition.FromNameOrEDName( commodityLocalizedName ) );
            }
        }
        
        private static StarSystem GetStarSystem ( string systemInput, TimeSpan timeout )
        {
            // NOTE: This uses a blocking wait on async code due to Cottle library limitations.
            // The IFunction interface does not support async operations, forcing us to block.
            // This is a known limitation that could be improved if Cottle adds async support.
            return GetStarSystemAsync( systemInput ).GetResultOrTimeout( timeout );
        }
        
        private static async Task<StarSystem> GetStarSystemAsync ( string systemInput )
        {
            if ( ulong.TryParse( systemInput, out var systemAddress ) )
            {
                return await EDDI.Instance.DataProvider
                    .GetOrFetchStarSystemAsync( systemAddress, true, true, true )
                    .ConfigureAwait( false );
            }
            
            return await EDDI.Instance.DataProvider
                .GetOrFetchStarSystemAsync( systemInput?.Trim(), true, true, true )
                .ConfigureAwait( false );
        }
    }
}
