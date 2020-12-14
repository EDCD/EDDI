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
        public string description => Properties.CustomFunctions_Untranslated.Occasionally;
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
