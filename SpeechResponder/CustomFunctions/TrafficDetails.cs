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
        public string description => @"
This function will provide information on traffic and hostilities in a star system.

TrafficDetails() takes one mandatory argument and one optional argument.

The first mandatory argument is the name of the star system. The second optional argument defines different data sets that are available:

* `traffic` the number of ships that have passed through the star system (this is the default if no second argument is provided)
* `deaths` the number of ships passing through the star system which have been destroyed
* `hostility` the percent of ships passing through the star system which have been destroyed

    The returned `Traffic` object contains properties representing various timespans: `day`, `week` and `total`.

Common usage is to provide information about traffic and hostilities within a star system, for example:

    {set trafficDetails to TrafficDetails(system.name)}
    {if trafficDetails.day > 0: At least {trafficDetails.day} ships have passed through {system.name} today. }

    {set deathDetails to TrafficDetails(system.name, ""deaths"")}
    {if deathDetails.week > 0: At least {deathDetails.week} ships have been destroyed in {system.name} this week. }";
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
