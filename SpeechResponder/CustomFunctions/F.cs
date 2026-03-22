using Cottle;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    [ method: UsedImplicitly]
    public class F ( IContext context, Dictionary<string, Script> scripts )
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
