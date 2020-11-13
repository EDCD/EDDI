using System.Collections.Generic;
using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using GalnetMonitor;

namespace EddiSpeechResponder.CustomFunctions
{
    public class GalnetNewsArticles : ICustomFunction
    {
        public string name => "GalnetNewsArticles";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => @"
This function will provide full information for a collection of Galnet articles.

GalnetNewsArticles() takes up to two optional arguments. By default it returns a collection of unread articles. 
    - The first optional argument is a string to filter the results and only return those from a named category (You may use ""All"" if you'd like to omit this filter).
    - The second optional argument is a boolean value which should be set to true if you'd like to retrieve all articles rather than all unread articles.";
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
