using Cottle.Functions;
using Cottle.Stores;

namespace EddiSpeechResponder.Service
{
    public interface ICustomFunction
    {
        string name { get; }
        FunctionCategory Category { get; }
        string description { get; }
        NativeFunction function { get; }
    }

    public enum FunctionCategory
    {
        Details,
        Dynamic,
        Galnet,
        Hidden,
        Phonetic,
        Tempo,
        Utility,
        Voice
    }

    public abstract class ResolverInstance<T1, T2>
    {
        protected ScriptResolver resolver { get; }
        protected BuiltinStore store { get; }

        protected ResolverInstance(T1 parameter1, T2 parameter2)
        {
            if (parameter1 is ScriptResolver resolverParameter)
            {
                resolver = resolverParameter;
            }
            if (parameter2 is BuiltinStore storeParameter)
            {
                store = storeParameter;
            }
        }
    }
}
