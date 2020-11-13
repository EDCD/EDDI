using Cottle.Functions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class SpeechVolume : ICustomFunction
    {
        public string name => "SpeechVolume";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => @"
This function allows you to dynamically adjust the volume of the spoken speech. This function uses SSML tags.

##### Please take care with decibel values. If accidentally you blow out your speakers, that's totally on you. 
SpeechRate() takes two mandatory arguments: the text to speak and the valume at which to speak it (legal values for the speech volume include ""silent"", ""x-soft"", ""soft"", ""medium"", ""loud"", ""x-loud"", ""default"", as well as relative decibel values like ""-6dB"").
A value of ""+0dB"" means no change of volume, ""+6dB"" means approximately twice the current amplitude, ""-6dB"" means approximately half the current amplitude.

Common usage of this is to provide a more human-sounding reading of text with variation in speech volume:

    {SpeechVolume('The quick brown fox', 'loud')}
    {SpeechVolume('jumped over the lazy dog', 'x-soft')}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                string volume = values[1].AsString ?? "default";
                return @"<prosody volume=""" + volume + @""">" + text + "</prosody>";
            }
            return "The SpeechVolume function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
