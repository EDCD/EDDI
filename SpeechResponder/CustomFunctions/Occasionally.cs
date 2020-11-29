using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Occasionally : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "Occasionally";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => @"
This function will take its argument 1/*n*th of the time, the rest of time discarding it.

Occasionally() takes two arguments: n, and the text argument.

Note that Occasionally() works on random numbers rather than counters, so in the below example the additional text will not show up every 7th time you boost but will show up on average 1/7 of the times that you boost.

Common usage of this is to provide additional text that is said now and again but would become irritating if said all the time, for example:

    Boost engaged.  {Occasionally(7, \""Hold on to something.\"")}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (resolver.random.Next((int)values[0].AsNumber) == 0)
            {
                return resolver.resolveFromValue(values[1].AsString, store, false);
            }
            else
            {
                return "";
            }
        }, 2);

        // Implement nesting
        public Occasionally(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
