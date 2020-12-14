using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EmpireRatingDetails : ICustomFunction
    {
        public string name => "EmpireRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EmpireRatingDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            EmpireRating result = EmpireRating.FromName(values[0].AsString);
            if (result == null)
            {
                result = EmpireRating.FromEDName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
