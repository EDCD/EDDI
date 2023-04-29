using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class FederationRatingDetails : ICustomFunction
    {
        public string name => "FederationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.FederationRatingDetails;
        public Type ReturnType => typeof( FederationRating );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = FederationRating.FromName(values[0].AsString) ?? 
                                       FederationRating.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
