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
        public string description => Properties.CustomFunctions_Untranslated.OneOf;
        public NativeFunction function => new NativeFunction((values) =>
        {
            return resolver?.resolveFromValue(values[resolver.random.Next(values.Count)].AsString, store, false);
        });

        // Implement nesting
        public OneOf(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
