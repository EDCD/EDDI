using Cottle;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsDelete : ICustomFunction
    {
        public string name => "GalnetNewsDelete";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsDelete;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, uuid ) =>
        {
            GalnetNewsProvider.Instance?.DeleteArticle(uuid.AsString);
            return "";
        });
    }
}
