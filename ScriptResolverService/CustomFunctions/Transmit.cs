using Cottle;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class Transmit ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts )
        : RecursiveFunction( context, scripts ), ICustomFunction
    {
        public string name => "Transmit";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.Transmit;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            if (!string.IsNullOrEmpty( input.AsString) )
            {
                var result = @"<transmit>" + input.AsString + "</transmit>";
                return ScriptResolver.resolveFromValue( result, GetContext( runtime.Globals ), false );
            }
            return "The Transmit function is used improperly. Please review the documentation for correct usage.";
        });
    }
}
