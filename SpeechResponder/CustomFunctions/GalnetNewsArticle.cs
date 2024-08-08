using Cottle;
using EddiDataDefinitions;
using EddiGalnetMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GalnetNewsArticle : ICustomFunction
    {
        public string name => "GalnetNewsArticle";
        public FunctionCategory Category => FunctionCategory.Galnet;
        public string description => Properties.CustomFunctions_Untranslated.GalnetNewsArticle;
        public Type ReturnType => typeof( News );
        public IFunction function => Function.CreatePure1( ( runtime, uuid ) =>
        {
            var result = GalnetSqLiteRepository.Instance.GetArticle(uuid.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
