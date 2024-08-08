using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StateDetails : ICustomFunction
    {
        public string name => "StateDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.StateDetails;
        public Type ReturnType => typeof( FactionState );
        public IFunction function => Function.CreateNative1( ( runtime, factionState, writer ) =>
        {
            var result = FactionState.FromName(factionState.AsString) ?? 
                                    FactionState.FromEDName(factionState.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
