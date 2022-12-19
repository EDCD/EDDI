using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System.Linq;

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
                if (SpeechService.Instance?.allVoices != null)
                {
                    return new ReflectionValue(
                        SpeechService.Instance.allVoices.FirstOrDefault(v =>
                            v.name == SpeechService.Instance.Configuration.StandardVoice) ?? new object());
                }
            }

            if (values.Count == 1)
            {
                if (int.TryParse(values[0].AsString, out var seed) && SpeechService.Instance?.allVoices != null)
                {
                    var fromSeed = new System.Random(seed);
                    return new ReflectionValue(SpeechService.Instance.allVoices
                        .OrderBy(o => fromSeed.Next()).ToList());
                }

                if (!string.IsNullOrEmpty(values[0].AsString) && SpeechService.Instance?.allVoices != null)
                {
                    foreach (var vc in SpeechService.Instance.allVoices)
                    {
                        if (vc.name.ToLowerInvariant().Contains(values[0].AsString.ToLowerInvariant()))
                        {
                            return new ReflectionValue(vc);
                        }
                    }

                    return $"Voice \"{values[0].AsString}\" not found.";
                }
            }

            return "The VoiceDetails function is used improperly. Please review the documentation for correct usage.";
        }, 0, 1);
    }
}
