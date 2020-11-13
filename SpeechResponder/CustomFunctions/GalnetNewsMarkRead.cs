using Cottle.Functions;
using EddiSpeechResponder.Service;
using GalnetMonitor;

namespace EddiSpeechResponder.CustomFunctions
{
    public class GalnetNewsMarkRead : ICustomFunction
    {
        public string name => "GalnetNewsMarkRead";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => @"
This function will mark a Galnet article as read.

It takes a single mandatory argument, the article uuid to mark as read.";
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
