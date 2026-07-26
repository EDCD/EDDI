using Cottle;
using EddiCore;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Reflection;
using Utilities;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class Distance : ICustomFunction
    {
        public string name => "Distance";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Distance;
        public Type ReturnType => typeof( decimal? );
        public IFunction function => Function.CreateNativeVariadic( ( runtime, values, writer ) =>
        {
            var numVal = values[0].Type == ValueContent.Number;
            var stringVal = values[0].Type == ValueContent.String;

            NavWaypoint curr = null;
            NavWaypoint dest = null;
            if (values.Count == 1 && stringVal)
            {
                curr = new NavWaypoint( EDDI.Instance.GameState.CurrentStarSystem );
                dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypointAsync(values[0].AsString).GetAwaiter().GetResult();
            }
            else if (values.Count == 2 && stringVal)
            {
                curr = EDDI.Instance.DataProvider.GetOrFetchSystemWaypointAsync( values[ 0 ].AsString ).GetAwaiter().GetResult();
                dest = EDDI.Instance.DataProvider.GetOrFetchSystemWaypointAsync( values[ 1 ].AsString ).GetAwaiter().GetResult();
            }
            if (curr != null && dest != null)
            {
                var result = dest.DistanceFromStarSystem(curr);
                if (result is null)
                {
                    return $"Unable to calculate distance between {curr.systemName} and {dest.systemName}. Could not obtain system coordinates.";
                }
                return Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            else if (values.Count == 6 && numVal)
            {
                var x1 = Convert.ToDecimal( values[ 0 ].AsNumber );
                var y1 = Convert.ToDecimal( values[ 1 ].AsNumber );
                var z1 = Convert.ToDecimal( values[ 2 ].AsNumber );
                var x2 = Convert.ToDecimal( values[ 3 ].AsNumber );
                var y2 = Convert.ToDecimal( values[ 4 ].AsNumber );
                var z2 = Convert.ToDecimal( values[ 5 ].AsNumber );
                var result = Functions.StellarDistanceLy( x1, y1, z1, x2, y2, z2 );
                return Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            else
            {
                return "The Distance function is used improperly. Please review the documentation for correct usage.";
            }
        });
    }
}
