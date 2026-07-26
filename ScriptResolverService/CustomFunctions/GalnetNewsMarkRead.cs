using Cottle;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsMarkRead : ICustomFunction
    {
        public string name => "GalnetNewsMarkRead";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsMarkRead;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, uuid ) =>
        {
            GalnetNewsProvider.Instance?.MarkArticleRead(uuid.AsString);
            return "";
        });
    }
}
