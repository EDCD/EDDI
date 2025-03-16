using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Reflection;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SystemDetails : ICustomFunction
    {
        public string name => "SystemDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SystemDetails;
        public Type ReturnType => typeof( StarSystem );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            try
            {
                StarSystem result;
                if ( values.Count == 0 )
                {
                    result = EDDI.Instance.CurrentStarSystem;
                }
                else if ( ulong.TryParse( values[ 0 ].AsString, out var systemAddress ) )
                {
                    result = EDDI.Instance.DataProvider.GetOrFetchStarSystem( systemAddress, true, true );
                }
                else
                {
                    result = EDDI.Instance.DataProvider.GetOrFetchStarSystem( values[ 0 ].AsString, true, true );
                }

                var distanceFromHome = result?.DistanceFromStarSystem(EDDI.Instance.HomeStarSystem);
                if (distanceFromHome != null)
                {
                    Logging.Debug("Distance from home is " + distanceFromHome);
                    result.distancefromhome = distanceFromHome;
                }

                return result is null 
                    ? Value.EmptyMap 
                    : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            catch (Exception e)
            {
                return $"The SystemDetails function is used incorrectly. {e.Message}.";
            }
        }, 0, 1);
    }
}
