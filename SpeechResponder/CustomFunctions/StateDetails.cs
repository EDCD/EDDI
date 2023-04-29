using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StateDetails : ICustomFunction
    {
        public string name => "StateDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.StateDetails;
        public Type ReturnType => typeof( FactionState );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = FactionState.FromName(values[0].AsString) ?? 
                                    FactionState.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
