using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class TrafficDetails : ICustomFunction
    {
        public string name => "TrafficDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.TrafficDetails;
        public Type ReturnType => typeof( Traffic );

        private static readonly DataProviderService dataProviderService = new DataProviderService();

        public NativeFunction function => new NativeFunction((values) =>
        {
            Traffic result = null;
            string systemName = values[0].AsString;
            if (!string.IsNullOrEmpty(systemName))
            {
                if (values.Count == 2)
                {
                    if (values[1].AsString == "traffic")
                    {
                        result = dataProviderService.GetSystemTraffic(systemName);
                    }
                    if (values[1].AsString == "deaths")
                    {
                        result = dataProviderService.GetSystemDeaths(systemName);
                    }
                    else if (values[1].AsString == "hostility")
                    {
                        result = dataProviderService.GetSystemHostility(systemName);
                    }
                }
                if (result == null)
                {
                    result = dataProviderService.GetSystemTraffic(systemName);
                }
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 2);
    }
}
