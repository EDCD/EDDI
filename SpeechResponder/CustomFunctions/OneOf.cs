using Cottle;
using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class OneOf : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "OneOf";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => Properties.CustomFunctions_Untranslated.OneOf;
        public Type ReturnType => typeof( string );
        private static readonly Random random = new Random();

        public NativeFunction function => new NativeFunction( ( values ) =>
        {
            lock ( random )
            {
                if ( values.Count == 1 && values[ 0 ].Type == ValueContent.Map )
                {
                    values[ 0 ].Fields.TryGet( random.Next( values[ 0 ].Fields.Count ), out var result );
                    return resolver?.resolveFromValue( result?.AsString, store, false );
                }
                else
                {
                    return resolver?.resolveFromValue( values[ random.Next( values.Count ) ]?.AsString, store, false );
                }
            }
        } );

        // Implement nesting
        public OneOf(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
