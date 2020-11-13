using Cottle.Functions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class Emphasize : ICustomFunction
    {
        public string name => "Emphasize";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => @"
This function allows you to give emphasis to specific words (to the extent supported by the voice you are using - your mileage may vary). This function uses SSML tags.

Emphasize() takes one mandatory argument: the text to speak with emphasis. If no secondary argument is specified, it shall default to a strong emphasis.

Emphasize() also takes one optional argument: the degree of emphasis to place on the text (legal values for the degree of emphasis include ""strong"", ""moderate"", ""none"" and ""reduced"").

Common usage of this is to provide a more human-sounding reading of text by allowing the application of emphasis:

    That is a {Emphasize('beautiful', 'strong')} ship you have there.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values.Count == 1)
            {
                return @"<emphasis level =""strong"">" + values[0].AsString + @"</emphasis>";
            }
            if (values.Count == 2)
            {
                return @"<emphasis level =""" + values[1].AsString + @""">" + values[0].AsString + @"</emphasis>";
            }
            return "The Emphasize function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
