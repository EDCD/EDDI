using Cottle.Functions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class SpeechRate : ICustomFunction
    {
        public string name => "SpeechRate";
        public FunctionCategory Category => FunctionCategory.Tempo;
        public string description => @"
This function allows you to dynamically adjust the rate of the spoken speech. This function uses SSML tags.

SpeechRate() takes two mandatory arguments: the text to speak and the speech rate at which to speak it (legal values for the speech rate include ""x-slow"", ""slow"", ""medium"", ""fast"", ""x-fast"", ""default"", as well as percentage values like ""-20%"" or ""+20%"").

Common usage of this is to provide a more human-sounding reading of text with variation in the speech rate:

    {SpeechRate('The quick brown fox', 'x-slow')}
    {SpeechRate('jumped over the lazy dog', 'fast')}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                string rate = values[1].AsString ?? "default";
                return @"<prosody rate=""" + rate + @""">" + text + "</prosody>";
            }
            return "The SpeechRate function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
