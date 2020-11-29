using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using GalnetMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsArticle : ICustomFunction
    {
        public string name => "GalnetNewsArticle";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => @"
This function will provide full information for a Galnet article given its uuid.

GalnetNewsArticle() takes a single argument of the article uuid for which you want more information.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
