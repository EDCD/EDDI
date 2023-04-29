using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ExplorationRatingDetails : ICustomFunction
    {
        public string name => "ExplorationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.ExplorationRatingDetails;
        public Type ReturnType => typeof( ExplorationRating );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = ExplorationRating.FromName(values[0].AsString) ?? 
                                        ExplorationRating.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
