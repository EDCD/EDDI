using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechVolume : ICustomFunction
    {
        public string name => "SpeechVolume";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.SpeechVolume;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var text = values[0].AsString;
            if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
            {
                return text;
            }
            if (values.Count == 2)
            {
                var volume = values[1].AsString;
                return @"<prosody volume=""" + volume + @""">" + text + "</prosody>";
            }
            return "The SpeechVolume function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
