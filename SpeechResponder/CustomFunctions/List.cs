using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class List : ICustomFunction
    {
        public string name => "List";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.List;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, values, writer ) =>
        {
            var output = string.Empty;
            var localisedAnd = Properties.SpeechResponder.localizedAnd;
            foreach ( var value in values.Fields )
            {
                var valueString = value.Value.AsString;
                if ( value.Key == 0 )
                {
                    output = valueString;
                }
                else if ( value.Key < ( values.Fields.Count - 1 ) )
                {
                    output = $"{output}, {valueString}";
                }
                else
                {
                    output = $"{output}{( values.Fields.Count() > 2 ? "," : "" )} {localisedAnd} {valueString}";
                }
            }
            return output;
        });
    }
}
