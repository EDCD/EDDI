using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class F : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "F";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function is used inside a script to invoke another script.

F() takes a single parameter that is the name of the script to invoke.

One example of its use is in the script for the event `Trade Promotion`:

    You have been recognised for your trading ability, {F(\""Honorific\"")}.

Here the call to script `Honorific` will generate the right title for the player, according to their allegiance.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            return resolver.resolveFromName(values[0].AsString, store, false);
        }, 1);
        
        // Implement nesting
        public F(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
