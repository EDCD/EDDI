using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using EddiDataProviderService;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class MaterialDetails : ICustomFunction
    {
        public string name => "MaterialDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MaterialDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Material result = Material.FromName(values[0].AsString);
            if (result?.edname != null && values.Count == 2)
            {
                StarSystem starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                if (starSystem != null)
                {
                    Body body = Material.highestPercentBody(result.edname, starSystem.bodies);
                    result.bodyname = body.bodyname;
                    result.bodyshortname = body.shortname;
                }
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 2);
    }
}
