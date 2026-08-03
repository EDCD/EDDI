using Cottle;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class P : ICustomFunction
    {
        public string name => "P";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.P;
        public Type ReturnType => typeof( string );
        public IFunction function => PronounceForContext.CreateFunction();
    }
}
