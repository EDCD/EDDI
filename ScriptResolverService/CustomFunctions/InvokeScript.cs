using Cottle;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class InvokeScript ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts )
        : RecursiveFunction( context, scripts ), ICustomFunction
    {
        public string name => "InvokeScript";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.InvokeScript;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
            base.InvokeScript( runtime.Globals, values ), 1, 2 );
    }
}
