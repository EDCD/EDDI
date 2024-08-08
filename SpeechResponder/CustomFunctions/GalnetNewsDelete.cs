using Cottle;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
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
            var result = GalnetSqLiteRepository.Instance.GetArticle(uuid.AsString);
            if (result != null)
            {
                GalnetSqLiteRepository.Instance.DeleteNews(result);
            }
            return "";
        });
    }
}
