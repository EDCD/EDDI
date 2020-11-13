using System.Collections.Generic;
using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using EddiSpeechService;

namespace EddiSpeechResponder.CustomFunctions
{
    public class VoiceDetails : ICustomFunction
    {
        public string name => "VoiceDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function allows you to discover details about the voices installed on your system. It is intended for use with `Voice()` to allow for more dynamic voice selection.

VoiceDetails takes either zero or one arguments.

With zero arguments, the function returns a list of `VoiceDetail` objects. For example:

{for voice in VoiceDetails(): \{voice.name\} speaks \{voice.culturename\},}

With one argument, the function returns a single `VoiceDetail` object. For example:

    {VoiceDetails(""Microsoft Zira Desktop"").culturename}
    {VoiceDetails(""Microsoft Zira Desktop"").gender}";
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
        private string name { get; set; }
        private string cultureinvariantname { get; set; }
        private string culturename { get; set; }
        private string culturecode { get; set; }
        private string gender { get; set; }
        public bool enabled { get; set; }

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
