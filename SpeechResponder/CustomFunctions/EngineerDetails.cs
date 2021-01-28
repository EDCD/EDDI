using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EngineerDetails : ICustomFunction
    {
        public string name => "EngineerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EngineerDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Engineer result = Engineer.FromName(values[0].AsString) ?? Engineer.FromSystemName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
