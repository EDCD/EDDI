using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class ExplorationRatingDetails : ICustomFunction
    {
        public string name => "ExplorationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for an exploration rating given its name.

ExplorationRatingDetails() takes a single argument of the exploration rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {ExplorationRatingDetails(""Surveyor"").rank} times.";
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
