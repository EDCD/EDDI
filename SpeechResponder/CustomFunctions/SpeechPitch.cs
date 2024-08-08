using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechPitch : ICustomFunction
    {
        public string name => "SpeechPitch";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.SpeechPitch;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            string text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                string pitch = values[1].AsString;
                return @"<prosody pitch=""" + pitch + @""">" + text + "</prosody>";
            }
            return "The SpeechPitch function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
