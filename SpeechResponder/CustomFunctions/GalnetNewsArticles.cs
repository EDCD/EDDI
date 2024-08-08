using Cottle;
using EddiDataDefinitions;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    internal class GalnetNewsArticles : ICustomFunction
    {
        public string name => "GalnetNewsArticles";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsArticles;
        public Type ReturnType => typeof( List<News> );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
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
            return results is null ? Value.EmptyMap : Value.FromReflection( results, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 0, 2);
    }
}
