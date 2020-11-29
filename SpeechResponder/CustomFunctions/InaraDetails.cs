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
        public string description => @"
This function will provide records from https://inara.cz for commanders with profiles on that website. Some values may be missing, depending on the completeness of the records and on the commander's sharing settings on https://inara.cz.

InaraDetails() takes one argument: the name of the commander to look up on Inara.cz.

Common usage of this is to provide details about other commanders. See the 'inaracmdr' object for variable details.";
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
