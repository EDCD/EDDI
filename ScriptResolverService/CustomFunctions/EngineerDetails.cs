using Cottle;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiScriptResolverService.CustomFunctions
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
            var result = GetEngineerDetails(input.AsString);
            return result is null
                ? Value.EmptyMap
                : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });

        private static Engineer GetEngineerDetails ( string input )
        {
            // Attempt to find the engineer by name
            var engineer = Engineer.FromName(input);
            if ( engineer != null )
            {
                return engineer;
            }

            // Attempt to find the engineer by system address
            if ( ulong.TryParse( input, out var systemAddress ) )
            {
                return Engineer.FromSystemAddress( systemAddress );
            }

            // Attempt to find the engineer by system name
            return Engineer.FromSystemName( input );
        }
    }
}
