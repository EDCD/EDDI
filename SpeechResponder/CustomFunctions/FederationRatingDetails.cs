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
        public string description => Properties.CustomFunctions_Untranslated.FederationRatingDetails;
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
