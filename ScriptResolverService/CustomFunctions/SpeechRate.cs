using Cottle;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class SpeechRate : ICustomFunction
    {
        public string name => "SpeechRate";
        public FunctionCategory Category => FunctionCategory.Tempo;
        public string description => Properties.CustomFunctions_Untranslated.SpeechRate;
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
                var rate = values[1].AsString;
                return @"<prosody rate=""" + rate + @""">" + text + "</prosody>";
            }
            return "The SpeechRate function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
