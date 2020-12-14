using Cottle.Functions;
using EddiSpeechResponder.Service;
using GalnetMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsMarkRead : ICustomFunction
    {
        public string name => "GalnetNewsMarkRead";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsMarkRead;
        public NativeFunction function => new NativeFunction((values) =>
        {
            News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
            if (result != null)
            {
                GalnetSqLiteRepository.Instance.MarkRead(result);
            }
            return "";
        }, 1);
    }
}
