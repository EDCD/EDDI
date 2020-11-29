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
        public string description => @"
This function will provide full information for a combat rating given its name.

CombatRatingDetails() takes a single argument of the combat rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {CombatRatingDetails(""Expert"").rank} times.";
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
