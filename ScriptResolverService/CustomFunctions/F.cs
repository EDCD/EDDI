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
        public IFunction function => Function.CreateNative1( ( runtime, scriptName, writer ) =>
        {
            var result = scriptName.AsString;
            return ScriptResolver.resolveFromName( result, Scripts, GetContext( runtime.Globals ), false )?.Trim();
        });
    }
}
