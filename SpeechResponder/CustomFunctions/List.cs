using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            string output = string.Empty;
            string localisedAnd = Properties.SpeechResponder.localizedAnd;
            if (values.Count == 1)
            {
                foreach (KeyValuePair<Cottle.Value, Cottle.Value> value in values[0].Fields)
                {
                    string valueString = value.Value.AsString;
                    if (value.Key == 0)
                    {
                        output = valueString;
                    }
                    else if (value.Key < (values[0].Fields.Count - 1))
                    {
                        output = $"{output}, {valueString}";
                    }
                    else
                    {
                        output = $"{output}{(values[0].Fields.Count() > 2 ? "," : "")} {localisedAnd} {valueString}";
                    }
                }
            }
            return output;
        }, 1);
    }
}
