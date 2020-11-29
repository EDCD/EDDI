using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class FederationRatingDetails : ICustomFunction
    {
        public string name => "FederationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for an federation rating given its name.

FederationRatingDetails() takes a single argument of the Federation rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {FederationRatingDetails(""Post Commander"").rank} times.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            FederationRating result = FederationRating.FromName(values[0].AsString);
            if (result == null)
            {
                result = FederationRating.FromEDName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
