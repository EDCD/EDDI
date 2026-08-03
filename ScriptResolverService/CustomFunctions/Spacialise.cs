using Cottle;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class Spacialise : ICustomFunction
    {
        public string name => "Spacialise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Spacialise;
        public Type ReturnType => typeof( string );
        public IFunction function => SpellOut.CreateFunction();
    }
}
