using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechVolume : ICustomFunction
    {
        public string name => "SpeechVolume";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.SpeechVolume;
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
