using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsArticle : ICustomFunction
    {
        public string name => "GalnetNewsArticle";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsArticle;
        public Type ReturnType => typeof( News );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
