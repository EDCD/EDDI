using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EmpireRatingDetails : ICustomFunction
    {
        public string name => "EmpireRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EmpireRatingDetails;
        public Type ReturnType => typeof( EmpireRating );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = EmpireRating.FromName(values[0].AsString) ?? 
                                    EmpireRating.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
