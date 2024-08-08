using Cottle;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System;
using System.Linq;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [JetBrains.Annotations.UsedImplicitly]
    public class VoiceDetails : ICustomFunction
    {
        public string name => "VoiceDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.VoiceDetails;
        public Type ReturnType => typeof( EddiSpeechService.VoiceDetails );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            if (values.Count == 0)
            {
                if (SpeechService.Instance?.allVoices != null)
                {
                    var result = SpeechService.Instance.allVoices.FirstOrDefault( v =>
                        v.name == SpeechService.Instance.Configuration.StandardVoice );
                    return result is null
                        ? Value.EmptyMap
                        : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                }
            }

            if (values.Count == 1)
            {
                if (int.TryParse(values[0].AsString, out var seed) && SpeechService.Instance?.allVoices != null)
                {
                    var fromSeed = new System.Random(seed);
                    var result = SpeechService.Instance.allVoices
                        .OrderBy( o => fromSeed.Next() ).ToList();
                    return Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                }

                if (!string.IsNullOrEmpty(values[0].AsString) && SpeechService.Instance?.allVoices != null)
                {
                    foreach (var result in SpeechService.Instance.allVoices)
                    {
                        if (result.name.ToLowerInvariant().Contains(values[0].AsString.ToLowerInvariant()))
                        {
                            return Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                        }
                    }

                    return $"Voice \"{values[0].AsString}\" not found.";
                }
            }

            return "The VoiceDetails function is used improperly. Please review the documentation for correct usage.";
        }, 0, 1);
    }
}
