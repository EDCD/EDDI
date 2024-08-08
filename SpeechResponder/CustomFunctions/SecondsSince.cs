using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SecondsSince : ICustomFunction
    {
        public string name => "SecondsSince";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SecondsSince;
        public Type ReturnType => typeof( long? );
        public IFunction function => Function.CreatePureMinMax( ( runtime, values ) =>
        {
            var date = values.Count == 1 
                ? (long)values[0].AsNumber 
                : 0;
            var now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);

            return Value.FromNumber( now - date );
        }, 0, 1);
    }
}
