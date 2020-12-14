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
        public string description => Properties.CustomFunctions_Untranslated.F;
        public NativeFunction function => new NativeFunction((values) =>
        {
            return resolver.resolveFromName(values[0].AsString, store, false);
        }, 1);
        
        // Implement nesting
        public F(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
