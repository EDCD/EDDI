using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Transmit : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "Transmit";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => @"
This function allows you to add a radio effect to speech.

Transmit() takes one argument: the text to speak. For example: 
             
    {Transmit(""{ShipName()} returning from orbit."")}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values.Count == 1)
            {
                return resolver.resolveFromValue(@"<transmit>" + values[0].AsString + "</transmit>", store, false);
            }
            return "The Transmit function is used improperly. Please review the documentation for correct usage.";
        }, 1);

        // Implement nesting
        public Transmit(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
