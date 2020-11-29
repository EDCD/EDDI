using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class BlueprintDetails : ICustomFunction
    {
        public string name => "BlueprintDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a blueprint, given its name and grade.

BlueprintDetails() takes two mandatory arguments: the name of the blueprint and the grade to retrieve. 

Common usage of this is to provide further information about a blueprint, for example:

    {set blueprint to BlueprintDetails(""Dirty Drive Tuning"", 5)}
    {len(blueprint.materials)} {if len(blueprint.materials) > 1: different materials are |else: material is} required to produce {blueprint.localizedName}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string blueprintName = values[0].AsString;
            int blueprintGrade = Convert.ToInt32(values[1].AsNumber);
            Blueprint result = Blueprint.FromNameAndGrade(blueprintName, blueprintGrade);
            return new ReflectionValue(result ?? new object());
        }, 2);
    }
}
