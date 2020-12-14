using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CombatRatingDetails : ICustomFunction
    {
        public string name => "CombatRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CombatRatingDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            CombatRating result = CombatRating.FromName(values[0].AsString);
            if (result == null)
            {
                result = CombatRating.FromEDName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
