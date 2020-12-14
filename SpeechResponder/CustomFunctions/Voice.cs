using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Voice : ICustomFunction
    {
        public string name => "Voice";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.Voice;
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
