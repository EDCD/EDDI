using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class TradeRatingDetails : ICustomFunction
    {
        public string name => "TradeRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a trade rating given its name.

TradeRatingDetails() takes a single argument of the trade rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {TradeRatingDetails(""Peddler"").rank} times.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            TradeRating result = TradeRating.FromName(values[0].AsString);
            if (result == null)
            {
                result = TradeRating.FromEDName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
