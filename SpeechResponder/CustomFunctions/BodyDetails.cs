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
    public class BodyDetails : ICustomFunction
    {
        public string name => "BodyDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.BodyDetails;
        public Type ReturnType => typeof( Body );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Body body;
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

                body = long.TryParse( values[ 0 ].AsString, out var bodyID )
                    ? system?.bodies?.Find( v => v.bodyId == bodyID )
                    : system?.bodies?.Find( v =>
                        v.bodyname?.ToLowerInvariant() == values[ 0 ].AsString?.ToLowerInvariant() );
            }
            else
            {
                body = EDDI.Instance.CurrentStellarBody;
            }

            return body is null
                ? Value.EmptyMap
                : Value.FromReflection( body, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
