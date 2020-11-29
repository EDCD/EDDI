using Cottle.Functions;
using EddiSpeechResponder.Service;
using GalnetMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsDelete : ICustomFunction
    {
        public string name => "GalnetNewsDelete";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => @"
This function will delete a Galnet article from local storage.

It takes a single mandatory argument, the article uuid to delete.";
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
