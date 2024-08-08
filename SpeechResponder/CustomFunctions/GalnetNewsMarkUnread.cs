using Cottle;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
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
            var result = GalnetSqLiteRepository.Instance.GetArticle(uuid.AsString);
            if (result != null)
            {
                GalnetSqLiteRepository.Instance.MarkUnread(result);
            }
            return "";
        });
    }
}
