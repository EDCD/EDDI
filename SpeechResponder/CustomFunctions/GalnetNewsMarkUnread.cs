using Cottle.Functions;
using EddiSpeechResponder.Service;
using GalnetMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsMarkUnread : ICustomFunction
    {
        public string name => "GalnetNewsMarkUnread";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsMarkUnread;
        public NativeFunction function => new NativeFunction((values) =>
        {
            News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
            if (result != null)
            {
                GalnetSqLiteRepository.Instance.MarkUnread(result);
            }
            return "";
        }, 1);
    }
}
