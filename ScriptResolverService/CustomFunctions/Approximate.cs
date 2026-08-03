using Cottle;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class Approximate ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts )
        : RecursiveFunction( context, scripts ), ICustomFunction
    {
        public string name => "Approximate";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Approximate;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
            RenderApproximateNumber( runtime.Globals, input ) );
    }
}
