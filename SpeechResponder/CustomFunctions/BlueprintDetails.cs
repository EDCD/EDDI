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
        public string description => Properties.CustomFunctions_Untranslated.BlueprintDetails; 
        public NativeFunction function => new NativeFunction((values) =>
        {
            string blueprintName = values[0].AsString;
            int blueprintGrade = Convert.ToInt32(values[1].AsNumber);
            Blueprint result = Blueprint.FromNameAndGrade(blueprintName, blueprintGrade);
            return new ReflectionValue(result ?? new object());
        }, 2);
    }
}
