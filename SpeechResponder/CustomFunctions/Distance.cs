using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Distance : ICustomFunction
    {
        public string name => "Distance";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Distance;
        public NativeFunction function => new NativeFunction((values) =>
        {
            bool numVal = values[0].Type == Cottle.ValueContent.Number;
            bool stringVal = values[0].Type == Cottle.ValueContent.String;

            StarSystem curr = null;
            StarSystem dest = null;
            if (values.Count == 1 && stringVal)
            {
                curr = EDDI.Instance?.CurrentStarSystem;
                dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
            }
            else if (values.Count == 2 && stringVal)
            {
                curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
            }
            if (curr != null && dest != null)
            {
                var result = curr.DistanceFromStarSystem(dest);
                if (result is null)
                {
                    return $"Unable to calculate distance between {curr.systemname} and {dest.systemname}. Could not obtain system coordinates.";
                }
                return new ReflectionValue(result);
            }
            else if (values.Count == 6 && numVal)
            {
                var x1 = (decimal)values[0].AsNumber;
                var y1 = (decimal)values[1].AsNumber;
                var z1 = (decimal)values[2].AsNumber;
                var x2 = (decimal)values[3].AsNumber;
                var y2 = (decimal)values[4].AsNumber;
                var z2 = (decimal)values[5].AsNumber;
                var result = Functions.DistanceFromCoordinates(x1, y1, z1, x2, y2, z2);
                return new ReflectionValue(result);
            }
            else
            {
                return "The Distance function is used improperly. Please review the documentation for correct usage.";
            }
        }, 1, 6);
    }
}
