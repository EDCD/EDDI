using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class BodyDetails : ICustomFunction
    {
        public string name => "BodyDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.BodyDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            StarSystem system;
            if (values.Count == 0)
            {
                system = EDDI.Instance.CurrentStarSystem;
            }
            else if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString) || (values.Count > 1 && values[1].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant()))
            {
                system = EDDI.Instance.CurrentStarSystem;
            }
            else
            {
                // Named system
                system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
            }
            Body result = system?.bodies?.Find(v => v.bodyname?.ToLowerInvariant() == values[0].AsString?.ToLowerInvariant());
            return new ReflectionValue(result ?? new object());
        }, 1, 2);
    }
}
