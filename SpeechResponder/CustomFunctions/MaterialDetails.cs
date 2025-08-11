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
    public class MaterialDetails : ICustomFunction
    {
        public string name => "MaterialDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MaterialDetails;
        public Type ReturnType => typeof( Material );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var result = Material.FromName(values[0].AsString);
            if (result?.edname != null && values.Count == 2)
            {
                var starSystem = EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( values[1].AsString, true, false ).GetAwaiter().GetResult();
                if (starSystem != null)
                {
                    var body = Material.highestPercentBody( result.edname, starSystem.bodies );
                    result.bodyname = body?.bodyname;
                    result.bodyshortname = body?.shortname;
                }
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
