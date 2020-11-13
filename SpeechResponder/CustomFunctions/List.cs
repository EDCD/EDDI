using Cottle.Functions;
using EddiSpeechResponder.Service;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    public class List : ICustomFunction
    {
        public string name => "List";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function will return a humanised list of items from an array (e.g. this, that, and the other thing).

List() takes a single argument, the array variable with items you want listed.

Common usage is to convert an array to a list, for example:

    {set systemsrepaired to ['the hull', 'the cockpit', 'corroded systems']}
    The limpet has repaired {List(systemsrepaired)}.";
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
