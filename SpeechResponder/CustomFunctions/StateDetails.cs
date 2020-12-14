using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StateDetails : ICustomFunction
    {
        public string name => "StateDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.StateDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            FactionState result = FactionState.FromName(values[0].AsString);
            if (result == null)
            {
                result = FactionState.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
