using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class MaterialDetails : ICustomFunction
    {
        public string name => "MaterialDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MaterialDetails;
        public Type ReturnType => typeof( Material );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var result = GetMaterialDetails(values);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);

        private Material GetMaterialDetails ( IReadOnlyList<Value> values )
        {
            // Attempt to find the material by name
            var material = Material.FromName(values[0].AsString);
            
            if ( material?.edname != null && values.Count == 2 )
            {
                // Try to fetch the star system
                try
                {
                    // NOTE: This uses a blocking wait on async code due to Cottle library limitations.
                    // The IFunction interface does not support async operations, forcing us to block.
                    // This is a known limitation that could be improved if Cottle adds async support.
                    var starSystem = GetMaterialStarSystem( values[ 1 ].AsString )
                        .GetResultOrTimeout( TimeSpan.FromSeconds( 10 ) );
                    if ( starSystem != null )
                    {
                        // If we successfully retrieved the star system, find the body with the highest material percentage
                        var body = Material.highestPercentBody(material.edname, starSystem.bodies);
                        material.bodyname = body?.bodyname;
                        material.bodyshortname = body?.shortname;
                    }
                }
                catch ( TimeoutException timee )
                {
                    Logging.Warn( $"Network request for {values[ 1 ].AsString} star system material data timed out.", timee );
                }
            }
            return material;
        }

        private static async Task<StarSystem> GetMaterialStarSystem ( string systemInput )
        {
            // Try to fetch by system address first
            if ( ulong.TryParse( systemInput, out var systemAddress ) )
            {
                return await EDDI.Instance.DataProvider
                    .GetOrFetchStarSystemAsync( systemAddress, true, false )
                    .ConfigureAwait( false );

            }

            // Try to fetch by star system name
            return await EDDI.Instance.DataProvider
                .GetOrFetchStarSystemAsync( systemInput.Trim(), true, false )
                .ConfigureAwait( false );
        }
    }
}
