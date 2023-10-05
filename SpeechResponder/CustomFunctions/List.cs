using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            string output = string.Empty;

            // Choose either And/Or for the last list item using the second argument (if any)
            var orList = values.Count > 1 && values[1].AsBoolean;
            var localisedAndOr = orList ? 
                Properties.SpeechResponder.localizedOr : 
                Properties.SpeechResponder.localizedAnd;

            var cottleMap = values[0].Fields;
            foreach ( KeyValuePair<Cottle.Value, Cottle.Value> value in cottleMap)
            {
                string valueString = value.Value.AsString;
                if (value.Key == 0)
                {
                    output = valueString;
                }
                else if (value.Key < ( cottleMap.Count - 1))
                {
                    output = $"{output}, {valueString}";
                }
                else
                {
                    output = $"{output}{( cottleMap.Count() > 2 ? "," : "")} {localisedAndOr} {valueString}";
                }
            }

            return output;
        }, 1, 2);
    }
}
