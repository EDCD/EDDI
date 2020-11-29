using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class OneOf : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "OneOf";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => @"
This function will take one of the arguments available to it, picking randomly.

OneOf() takes as many arguments are you want to give it.

Common usage of this is to provide variation to spoken text, for example:

    You have {OneOf(\""docked\"", \""finished docking\"", \""completed docking procedures\"")}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            return resolver?.resolveFromValue(values[resolver.random.Next(values.Count)].AsString, store, false);
        });

        // Implement nesting
        public OneOf(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
