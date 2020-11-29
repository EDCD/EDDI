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
        public string description => @"
This function will provide full information for a body given its name.

BodyDetails() takes a single mandatory argument of the name of the body for which you want more information.  If the body is not in the current system then it can be provided with a second parameter of the name of the system.

Common usage of this is to provide further information about a body, for example:

    {set body to BodyDetails(""Earth"", ""Sol"")}
    Earth is {body.distancefromstar} light years from the system's main star.";
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
