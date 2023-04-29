using Cottle.Functions;
using Cottle.Stores;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SetState : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "SetState";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SetState;
        public Type ReturnType => typeof( string );
        public NativeFunction function => new NativeFunction((values) =>
        {
            string varName = values[0].AsString.ToLowerInvariant().Replace(" ", "_");
            Cottle.Value value = values[1];
            if (value.Type == Cottle.ValueContent.Boolean)
            {
                EDDI.Instance.State[varName] = value.AsBoolean;
                store["state"] = ScriptResolver.buildState();
            }
            else if (value.Type == Cottle.ValueContent.Number)
            {
                EDDI.Instance.State[varName] = value.AsNumber;
                store["state"] = ScriptResolver.buildState();
            }
            else if (value.Type == Cottle.ValueContent.String)
            {
                EDDI.Instance.State[varName] = value.AsString;
                store["state"] = ScriptResolver.buildState();
            }
            else if (value.Type == Cottle.ValueContent.Void)
            {
                EDDI.Instance.State[varName] = null;
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
