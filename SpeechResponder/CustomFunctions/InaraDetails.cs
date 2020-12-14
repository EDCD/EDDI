using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class InaraDetails : ICustomFunction
    {
        public string name => "InaraDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.InaraDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values[0].AsString is string commanderName)
            {
                if (!string.IsNullOrWhiteSpace(commanderName))
                {
                    EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                    var result = inaraService.GetCommanderProfile(commanderName);
                    return new ReflectionValue(result ?? new object());
                }
            }
            return "";
        }, 1);
    }
}
