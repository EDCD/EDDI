using Cottle;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Linq;
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
                else
                {
                    var key = values[ 0 ].AsString;

                    // First attempt to resolve the system from the live game state, then fall back to the data provider if that fails.
                    // This allows us to resolve the current system even if it has not yet been added to the data provider.
                    result = ResolveLiveSystemFirst( key );

                    // If we didn't find a match in the live game state, try the data provider.
                    if ( result is null && ulong.TryParse( key, out var systemAddress ) )
                    {
                        result = EDDI.Instance.DataProvider
                            .GetOrFetchStarSystemAsync( systemAddress, true, true )
                            .GetAwaiter()
                            .GetResult();
                    }
                    else if ( result is null )
                    {
                        result = EDDI.Instance.DataProvider
                            .GetOrFetchStarSystemAsync( key, true, true )
                            .GetAwaiter()
                            .GetResult();
                    }
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

        private static bool MatchesSystem ( StarSystem system, string key, ulong? systemAddress )
        {
            if ( system is null ) { return false; }

            if ( systemAddress is > 0 && system.systemAddress == systemAddress )
            {
                return true;
            }

            return !string.IsNullOrEmpty( key ) && system.systemname.Equals( key, StringComparison.InvariantCultureIgnoreCase );
        }

        private static StarSystem ResolveLiveSystemFirst ( string key )
        {
            ulong.TryParse( key, out var parsedAddress );
            ulong? systemAddress = parsedAddress > 0 ? parsedAddress : null;

            var gameState = EDDI.Instance.GameState;

            return new[]
                {
                    gameState.CurrentStarSystem,
                    gameState.LastStarSystem,
                    gameState.NextStarSystem,
                    gameState.DestinationStarSystem,
                    NavigationService.Instance.SearchStarSystem
                }.FirstOrDefault( s => MatchesSystem( s, key, systemAddress ) );
        }
    }
}
