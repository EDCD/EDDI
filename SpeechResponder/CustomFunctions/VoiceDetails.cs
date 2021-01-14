using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
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
                if (SpeechService.Instance?.allVoices != null)
                {
                    foreach (var vc in SpeechService.Instance.allVoices)
                    {
                        if (!vc.name.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            voices.Add(new VoiceDetail(
                                vc.name,
                                vc.Culture.Parent.EnglishName,
                                vc.Culture.Parent.NativeName,
                                vc.Culture.Name,
                                vc.gender
                                ));
                        }
                    }
                }
                return new ReflectionValue(voices);
            }
            if (values.Count == 1)
            {
                VoiceDetail result = null;
                if (SpeechService.Instance?.allVoices != null && !string.IsNullOrEmpty(values[0].AsString))
                {
                    foreach (var vc in SpeechService.Instance.allVoices)
                    {
                        if (vc.name.ToLowerInvariant().Contains(values[0].AsString.ToLowerInvariant())
                        && !vc.name.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            result = new VoiceDetail(
                                vc.name,
                                vc.Culture.Parent.EnglishName,
                                vc.Culture.Parent.NativeName,
                                vc.Culture.Name,
                                vc.gender
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

        public VoiceDetail(string name, string cultureinvariantname, string culturename, string culturecode, string gender)
        {
            this.name = name;
            this.cultureinvariantname = cultureinvariantname;
            this.culturename = culturename;
            this.culturecode = culturecode;
            this.gender = gender;
        }
    }
}
