using Cottle.Functions;
using Cottle.Values;
using EddiCargoMonitor;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class HaulageDetails : ICustomFunction
    {
        public string name => "HaulageDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.HaulageDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor"))?.GetHaulageWithMissionId((long)values[0].AsNumber);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
