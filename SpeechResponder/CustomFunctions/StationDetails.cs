using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StationDetails : ICustomFunction
    {
        public string name => "StationDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.StationDetails;
        public Type ReturnType => typeof( Station );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Station station;
            if ( values.Any() )
            {
                StarSystem system;
                if ( values.Count < 2 || string.IsNullOrEmpty( values[ 1 ].AsString ) )
                {
                    // Current system
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    // Named system
                    system = ulong.TryParse( values[ 1 ].AsString, out var systemAddress )
                        ? EDDI.Instance.DataProvider.GetOrFetchStarSystem( systemAddress, true, false )
                        : EDDI.Instance.DataProvider.GetOrFetchStarSystem( values[ 1 ].AsString, true, false );
                }

                station = long.TryParse( values[ 0 ].AsString, out var marketID )
                    ? system?.stations?.Find( v => v.marketId == marketID )
                    : system?.stations?.Find( v =>
                        v.name?.ToLowerInvariant() == values[ 0 ].AsString?.ToLowerInvariant() );
            }
            else
            {
                station = EDDI.Instance.CurrentStation;
            }

            return station is null 
                ? Value.EmptyMap 
                : Value.FromReflection( station, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
