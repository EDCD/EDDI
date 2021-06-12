using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System.Collections.Generic;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [JetBrains.Annotations.UsedImplicitly]
    public class VoiceDetails : ICustomFunction
    {
        public string name => "VoiceDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.VoiceDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values.Count == 0)
            {
                List<VoiceDetail> voices = new List<VoiceDetail>();
                if (SpeechService.Instance?.synth != null)
                {
                    foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                    {
                        if (!vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            voices.Add(new VoiceDetail(
                                vc.VoiceInfo.Name,
                                vc.VoiceInfo.Culture.Parent.EnglishName,
                                vc.VoiceInfo.Culture.Parent.NativeName,
                                vc.VoiceInfo.Culture.Name,
                                vc.VoiceInfo.Gender.ToString(),
                                vc.Enabled
                                ));
                        }
                    }
                }
                return new ReflectionValue(voices);
            }
            if (values.Count == 1)
            {
                VoiceDetail result = null;
                if (SpeechService.Instance?.synth != null && !string.IsNullOrEmpty(values[0].AsString))
                {
                    foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                    {
                        if (vc.VoiceInfo.Name.ToLowerInvariant().Contains(values[0].AsString.ToLowerInvariant())
                        && !vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            result = new VoiceDetail(
                                vc.VoiceInfo.Name,
                                vc.VoiceInfo.Culture.Parent.EnglishName,
                                vc.VoiceInfo.Culture.Parent.NativeName,
                                vc.VoiceInfo.Culture.Name,
                                vc.VoiceInfo.Gender.ToString(),
                                vc.Enabled
                                );
                            break;
                        }
                    }
                }
                return new ReflectionValue(result ?? new object());
            }
            return "The VoiceDetails function is used improperly. Please review the documentation for correct usage.";
        }, 0, 1);
    }

    class VoiceDetail
    {
        [PublicAPI]
        public string name { get; }

        [PublicAPI]
        public string cultureinvariantname { get; }

        [PublicAPI]
        public string culturename { get; }

        [PublicAPI]
        public string culturecode { get; }

        [PublicAPI]
        public string gender { get; }

        public bool enabled { get; }

        public VoiceDetail(string name, string cultureinvariantname, string culturename, string culturecode, string gender, bool enabled)
        {
            this.name = name;
            this.cultureinvariantname = cultureinvariantname;
            this.culturename = culturename;
            this.culturecode = culturecode;
            this.gender = gender;
            this.enabled = enabled;
        }
    }
}
