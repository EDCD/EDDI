using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
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
                        result = EDDI.Instance.DataProvider.GetSystemTrafficAsync(systemName)?.GetAwaiter().GetResult();
                    }
                    if (values[1].AsString == "deaths")
                    {
                        result = EDDI.Instance.DataProvider.GetSystemDeathsAsync(systemName)?.GetAwaiter().GetResult();
                    }
                    else if (values[1].AsString == "hostility")
                    {
                        result = EDDI.Instance.DataProvider.GetSystemHostilityAsync(systemName)?.GetAwaiter().GetResult();
                    }
                }
                if (result == null)
                {
                    result = EDDI.Instance.DataProvider.GetSystemTrafficAsync(systemName)?.GetAwaiter().GetResult();
                }
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
