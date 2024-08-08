using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EconomyDetails : ICustomFunction
    {
        public string name => "EconomyDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EconomyDetails;
        public Type ReturnType => typeof( Economy );
        public IFunction function => Function.CreateNative1( ( runtime, economy, writer ) =>
        {
            var result = Economy.FromName(economy.AsString) ?? 
                                Economy.FromEDName(economy.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
