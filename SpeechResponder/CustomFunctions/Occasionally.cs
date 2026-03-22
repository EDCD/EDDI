using Cottle;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class Occasionally ( IContext context, Dictionary<string, Script> scripts )
        : RecursiveFunction( context, scripts ), ICustomFunction
    {
        public string name => "Occasionally";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => Properties.CustomFunctions_Untranslated.Occasionally;
        public Type ReturnType => typeof( string );

        private static readonly Random random =
            new( new { n = nameof(Occasionally), dt = DateTime.UtcNow }.GetHashCode() );

        public IFunction function => Function.CreateNative2( ( runtime, n, input, writer ) =>
        {
            Value result;
            if ( random.Next( Convert.ToInt32( n.AsNumber) ) == 0 )
            {
                result = input.AsString;
            }
            else
            {
                result = "";
            }
            return ScriptResolver.resolveFromValue( result.AsString, GetContext( runtime.Globals ), false );
        });
    }
}
