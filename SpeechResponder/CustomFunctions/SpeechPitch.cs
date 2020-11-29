using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechPitch : ICustomFunction
    {
        public string name => "SpeechPitch";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => @"
This function allows you to dynamically adjust the pitch of the spoken speech. This function uses SSML tags.

SpeechPitch() takes two mandatory arguments: the text to speak and the pitch at which to speak it (legal values for the pitch include ""x-low"", ""low"", ""medium"", ""high"", ""x-high"", ""default"", as well as percentage values like ""-20%"" or ""+10%"").

Common usage of this is to provide a more human-sounding reading of text with variation in the speech pitch:

    {SpeechPitch('Ok, who added helium to the life support unit?', 'high')}
    {Pause(1000)}
    {SpeechPitch('Countering with sodium hexa-flouride.', 'x-low')}
    Equilibrium restored.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                string pitch = values[1].AsString ?? "default";
                return @"<prosody pitch=""" + pitch + @""">" + text + "</prosody>";
            }
            return "The SpeechPitch function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
