using Cottle.Functions;
using Cottle.Stores;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class FactionDetails : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "FactionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a minor faction given its name.

FactionDetails() typically takes a single argument of the faction name, but may add a system name for filtering.

Common usage of this is to obtain a `Faction` object, providing current specifics of a minor faction, for example:

    {set faction to FactionDetails(""Lavigny's Legion"")}
    {if faction.name != """":
        {faction.name} is present in the
        {for presence in faction.presences:
            {presence.systemName},
        }
        {if len(faction.presences) = 1: system |else: systems}.
    }";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Faction result;
            if (values.Count == 0)
            {
                result = EDDI.Instance.CurrentStarSystem.Faction;
            }
            else if (values.Count == 1)
            {
                result = resolver.bgsService.GetFactionByName(values[0].AsString);
            }
            else
            {
                result = resolver.bgsService.GetFactionByName(values[0].AsString, values[1].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 2);

        // Implement nesting
        public FactionDetails(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
