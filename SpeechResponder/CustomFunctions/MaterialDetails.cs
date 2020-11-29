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
        public string description => @"
This function will provide full information for a material given its name.

MaterialDetails() takes either one or two arguments. 

The first argument is the name of the material for which you want more information. 

Common usage of this is to provide further information about a material, for example:

    Iron is a {MaterialDetails(""Iron"").rarity.name} material.

The second argument, the name of a star system, is optional. If provided then the `bodyname` and `bodyshortname` properties in the resulting `Material` object will return details from body with the highest concentration of the material within the specified star system.

Common usage of this is to provide recommendations for material gathering.

    {set materialName to ""Iron""}
    {set details to MaterialDetails(materialName, system.name)}
    The best place to find {materialName} in {system.name} is on {if details.bodyname != details.bodyshortname: body} {details.bodyshortname}.";
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
