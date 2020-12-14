using System.Collections.Generic;
using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using GalnetMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    internal class GalnetNewsArticles : ICustomFunction
    {
        public string name => "GalnetNewsArticles";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsArticles;
        public NativeFunction function => new NativeFunction((values) =>
        {
            List<News> results = null;
            if (values.Count == 0)
            {
                // Obtain all unread articles
                results = GalnetSqLiteRepository.Instance.GetArticles();
            }
            else if (values.Count == 1)
            {
                // Obtain all unread news of a given category
                results = GalnetSqLiteRepository.Instance.GetArticles(values[0].AsString);
            }
            else if (values.Count == 2)
            {
                // Obtain all news of a given category
                results = GalnetSqLiteRepository.Instance.GetArticles(values[0].AsString, values[1].AsBoolean);
            }
            return new ReflectionValue(results ?? new List<News>());
        }, 0, 2);
    }
}
