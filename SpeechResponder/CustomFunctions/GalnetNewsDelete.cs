using Cottle.Functions;
using EddiDataDefinitions;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
            if (result != null)
            {
                GalnetSqLiteRepository.Instance.DeleteNews(result);
            }
            return "";
        }, 1);
    }
}
