using Cottle.Functions;
using Cottle.Stores;
using EddiCore;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class SetState : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "SetState";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function will set a session state value.  The value will be available as a property of the 'state' object in future templates within the same EDDI session.

SetState takes two arguments: the name of the state value to set, and its value.  The name of the state value will be converted to lower-case and spaces changed to underscores.  The value must be either a boolean, a number, or a string; other values will be ignored.

Common usage of this is to keep track of the cumulative or persistent information within a session, for example:

    {SetState(""distance_travelled_today"", state.distance_travelled_today + event.distance)}";
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
