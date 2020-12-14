using Cottle.Functions;
using Cottle.Stores;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SetState : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "SetState";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SetState;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string name = values[0].AsString.ToLowerInvariant().Replace(" ", "_");
            Cottle.Value value = values[1];
            if (value.Type == Cottle.ValueContent.Boolean)
            {
                EDDI.Instance.State[name] = value.AsBoolean;
                store["state"] = ScriptResolver.buildState();
            }
            else if (value.Type == Cottle.ValueContent.Number)
            {
                EDDI.Instance.State[name] = value.AsNumber;
                store["state"] = ScriptResolver.buildState();
            }
            else if (value.Type == Cottle.ValueContent.String)
            {
                EDDI.Instance.State[name] = value.AsString;
                store["state"] = ScriptResolver.buildState();
            }
            // Ignore other possibilities
            return "";
        }, 2);

        // Implement nesting
        public SetState(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
