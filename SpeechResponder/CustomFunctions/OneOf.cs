using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class OneOf : RecursiveFunction, ICustomFunction
    {
        public string name => "OneOf";
        public FunctionCategory Category => FunctionCategory.Dynamic;
        public string description => Properties.CustomFunctions_Untranslated.OneOf;
        public Type ReturnType => typeof( string );

        private static readonly Random random =
            new Random( new { n = nameof(OneOf), dt = DateTime.UtcNow }.GetHashCode() );

        public IFunction function => Function.CreateNativeVariadic( ( runtime, values, writer ) =>
        {
            Value result;
            if ( values.Count == 1 && values[ 0 ].Type == ValueContent.Map )
            {
                values[ 0 ].Fields.TryGet( random.Next( values[ 0 ].Fields.Count ), out result );
            }
            else
            {
                var rand = random.Next( values.Count );
                result = values[ rand ];
            }
            return ScriptResolver.resolveFromValue( result.AsString, GetContext( runtime.Globals ), false );
        } );

        [UsedImplicitly]
        public OneOf ( IContext context, Dictionary<string, Script> scripts ) : base( context, scripts )
        { }
    }
}
