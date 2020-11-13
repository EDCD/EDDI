using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;

namespace EddiSpeechResponder.CustomFunctions
{
    public class Voice : ICustomFunction
    {
        public string name => "Voice";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => @"
This function allows you to include a different voice in your script than then one currently selected. This function uses SSML tags.

Voice() takes two mandatory arguments: the text to speak and the voice to speak it (legal values for the voice should match one of the voices listed by EDDI's `Text-to-Speech` tab.""). For Example:

    {Voice(""Now I can speak"", ""Microsoft Zira Desktop"")}
    {Voice(""And I can listen"", ""Microsoft David Desktop"")}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string text = values[0].AsString ?? string.Empty;
            string voice = values[1].AsString ?? string.Empty;

            if (SpeechService.Instance?.synth != null)
            {
                foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                {
                    if (vc.VoiceInfo.Name.ToLowerInvariant().Contains(voice?.ToLowerInvariant())
                        && !vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                    {
                        voice = vc.VoiceInfo.Name;
                        break;
                    }
                }
            }

            if (values.Count == 2)
            {
                return @"<voice name=""" + voice + @""">" + text + "</voice>";
            }
            return "The Voice function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
