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
            string localisedAndOr;

            Cottle.IMap cottleVal = values[0].Fields;

            // Let us choose either And/Or for the last list item
            string type = values.Count > 1 ? values[1].AsString : "and";
            type = type.ToLower();

            switch ( type )
            {
                case "or":
                    localisedAndOr = Properties.SpeechResponder.localizedOr;
                    break;
                //case "and":
                default:
                    localisedAndOr = Properties.SpeechResponder.localizedAnd;
                    break;
            }

            foreach (KeyValuePair<Cottle.Value, Cottle.Value> value in cottleVal)
            {
                string valueString = value.Value.AsString;
                if (value.Key == 0)
                {
                    output = valueString;
                }
                else if (value.Key < ( cottleVal.Count - 1))
                {
                    output = $"{output}, {valueString}";
                }
                else
                {
                    output = $"{output}{( cottleVal.Count() > 2 ? "," : "")} {localisedAndOr} {valueString}";
                }
            }

            return output;
        }, 1, 2);
    }
}
