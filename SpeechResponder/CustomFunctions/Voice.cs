using System;
using System.Linq;
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
            string voice = SpeechService.Instance.allvoices?.SingleOrDefault(v => string.Equals(v, values[1].AsString ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));
            if (values.Count == 2)
            {
                return @"<voice name=""" + voice + @""">" + text + "</voice>";
            }
            return "The Voice function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
