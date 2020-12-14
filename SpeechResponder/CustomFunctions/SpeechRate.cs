using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechRate : ICustomFunction
    {
        public string name => "SpeechRate";
        public FunctionCategory Category => FunctionCategory.Tempo;
        public string description => Properties.CustomFunctions_Untranslated.SpeechRate;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                string rate = values[1].AsString ?? "default";
                return @"<prosody rate=""" + rate + @""">" + text + "</prosody>";
            }
            return "The SpeechRate function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
