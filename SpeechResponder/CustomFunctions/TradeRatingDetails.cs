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
        public string description => Properties.CustomFunctions_Untranslated.TradeRatingDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = TradeRating.FromName(values[0].AsString) ?? 
                                   TradeRating.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
