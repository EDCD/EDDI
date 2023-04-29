using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    internal class GalnetNewsArticles : ICustomFunction
    {
        public string name => "GalnetNewsArticles";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsArticles;
        public Type ReturnType => typeof( List<News> );
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
