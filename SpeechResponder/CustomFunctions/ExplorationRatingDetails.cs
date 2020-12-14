using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ExplorationRatingDetails : ICustomFunction
    {
        public string name => "ExplorationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.ExplorationRatingDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            ExplorationRating result = ExplorationRating.FromName(values[0].AsString);
            if (result == null)
            {
                result = ExplorationRating.FromEDName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
