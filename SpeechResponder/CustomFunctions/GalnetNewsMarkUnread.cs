using Cottle;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsMarkUnread : ICustomFunction
    {
        public string name => "GalnetNewsMarkUnread";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsMarkUnread;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, uuid ) =>
        {
            GalnetNewsProvider.Instance?.MarkArticleUnread(uuid.AsString);
            return "";
        });
    }
}
