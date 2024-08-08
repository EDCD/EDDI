using Cottle;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

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

        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Traffic result = null;
            var systemName = values[0].AsString;
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
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
