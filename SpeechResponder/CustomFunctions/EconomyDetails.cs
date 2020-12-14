using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EconomyDetails : ICustomFunction
    {
        public string name => "EconomyDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EconomyDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Economy result = Economy.FromName(values[0].AsString);
            if (result == null)
            {
                result = Economy.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
