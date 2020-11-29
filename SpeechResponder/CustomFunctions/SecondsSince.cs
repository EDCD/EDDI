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
        public string description => @"
This function will provide the number of seconds since a given timestamp.

SecondsSince() takes a single argument of a UNIX timestamp.

Common usage of this is to check how long it has been since a given time, for example:

    Station data is {SecondsSince(station.updatedat) / 3600} hours old.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            long? date = (long?)values[0].AsNumber;
            long? now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);

            return new ReflectionValue(now - date);
        }, 1);
    }
}
