using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Occasionally : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "Occasionally";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => Properties.CustomFunctions_Untranslated.Occasionally;
        public Type ReturnType => typeof( string );

        private static readonly Random random = new Random();

        public NativeFunction function => new NativeFunction((values) =>
        {
            lock ( random )
            {
                if ( random.Next( (int)values[ 0 ].AsNumber ) == 0 )
                {
                    return values[ 1 ];
                }
                else
                {
                    return "";
                }
            }
        }, 2);

        // Implement nesting
        public Occasionally(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
