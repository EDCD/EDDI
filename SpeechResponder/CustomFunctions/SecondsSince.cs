using Cottle.Functions;
using Cottle.Values;
using EddiSpeechResponder.Service;
using System;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SecondsSince : ICustomFunction
    {
        public string name => "SecondsSince";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SecondsSince;
        public NativeFunction function => new NativeFunction((values) =>
        {
            long? date = (long?)values[0].AsNumber;
            long? now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);

            return new ReflectionValue(now - date);
        }, 1);
    }
}
