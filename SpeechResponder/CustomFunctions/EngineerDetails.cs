using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EngineerDetails : ICustomFunction
    {
        public string name => "EngineerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EngineerDetails;
        public Type ReturnType => typeof( Engineer );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var result = Engineer.FromName(input.AsString);
            if ( result is null )
            {
                var starSystem = ulong.TryParse( input.AsString, out var systemAddress ) 
                    ? EDDI.Instance.DataProvider.GetOrFetchStarSystem( systemAddress, true, true ) 
                    : EDDI.Instance.DataProvider.GetOrFetchStarSystem( input.AsString, true, true );

                if ( starSystem != null )
                {
                    result = Engineer.FromSystemAddress( starSystem.systemAddress );
                }
            }

            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
