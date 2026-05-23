using Cottle;
using EddiConfigService;
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
                    result = EDDI.Instance.GameState.CurrentStarSystem;
                }
                else if ( ulong.TryParse( values[ 0 ].AsString, out var systemAddress ) )
                {
                    result = EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( systemAddress, true, true ).GetAwaiter().GetResult();
                }
                else
                {
                    result = EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( values[ 0 ].AsString, true, true ).GetAwaiter().GetResult();
                }

                var commanderConfig = ConfigService.Instance.commanderConfiguration;
                var homeX = commanderConfig.homeSystemX;
                var homeY = commanderConfig.homeSystemY;
                var homeZ = commanderConfig.homeSystemZ;
                var distanceFromHome = result?.DistanceFromStarSystem(homeX, homeY, homeZ );

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
