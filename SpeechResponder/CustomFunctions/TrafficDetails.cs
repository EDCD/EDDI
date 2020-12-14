using Cottle.Functions;
using Cottle.Stores;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class TrafficDetails : ResolverInstance<ScriptResolver, BuiltinStore>, ICustomFunction
    {
        public string name => "TrafficDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.TrafficDetails;
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
                        result = resolver.dataProviderService.GetSystemTraffic(systemName);
                    }
                    if (values[1].AsString == "deaths")
                    {
                        result = resolver.dataProviderService.GetSystemDeaths(systemName);
                    }
                    else if (values[1].AsString == "hostility")
                    {
                        result = resolver.dataProviderService.GetSystemHostility(systemName);
                    }
                }
                if (result == null)
                {
                    result = resolver.dataProviderService.GetSystemTraffic(systemName);
                }
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 2);

        // Obtain the resolver
        public TrafficDetails(ScriptResolver resolver, BuiltinStore store) : base(resolver, store)
        { }
    }
}
