using Cottle.Functions;
using Cottle.Values;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            long? date = values.Count == 1 
                ? (long?)values[0].AsNumber 
                : 0;
            long? now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);

            return new ReflectionValue(now - date);
        }, 0, 1);
    }
}
