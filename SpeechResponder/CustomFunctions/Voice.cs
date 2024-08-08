using Cottle;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Voice : ICustomFunction
    {
        public string name => "Voice";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.Voice;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            string text = values[0].AsString;
            var voice = SpeechService.Instance.allVoices?.SingleOrDefault(v => string.Equals(v.name, values[1].AsString ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));
            if (voice != null && values.Count == 2)
            {
                return @"<voice name=""" + voice.name + @""" xml:lang=""" + voice.culturecode + @""">" + text + "</voice>";
            }
            else if (voice is null)
            {
                return $"Voice \"{values[1].AsString}\" was not found. Please review available voices from EDDI's Text-To-Speech tab.";
            }
            return "The Voice function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
