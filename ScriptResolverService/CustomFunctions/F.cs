using Cottle;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class F ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts )
        : RecursiveFunction( context, scripts ), ICustomFunction
    {
        public string name => "F";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.F;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var scriptName = values[ 0 ].AsString;
            if ( values.Count > 1 && values[ 1 ].Type != ValueContent.Map )
            {
                throw new ArgumentException ( $"The function invoking {scriptName} has arguments which are not a map value." );
            }

            return ScriptInvoker.ResolveFromName(
                scriptName,
                Scripts,
                GetContext( runtime.Globals ),
                false,
                false,
                values.Count > 1,
                values.Count > 1 ? values[ 1 ] : default )?.Trim();
        }, 1, 2 );
    }
}
