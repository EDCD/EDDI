using Cottle.Functions;
using Cottle.Stores;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class FactionDetails : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "FactionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.FactionDetails;
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
