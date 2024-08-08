using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
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

            StarSystem curr = null;
            StarSystem dest = null;
            if (values.Count == 1 && stringVal)
            {
                curr = EDDI.Instance?.CurrentStarSystem;
                dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
            }
            else if (values.Count == 2 && stringVal)
            {
                curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
            }
            if (curr != null && dest != null)
            {
                var result = curr.DistanceFromStarSystem(dest);
                if (result is null)
                {
                    return $"Unable to calculate distance between {curr.systemname} and {dest.systemname}. Could not obtain system coordinates.";
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
