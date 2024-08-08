using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class FederationRatingDetails : ICustomFunction
    {
        public string name => "FederationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.FederationRatingDetails;
        public Type ReturnType => typeof( FederationRating );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var result = input.Type == ValueContent.Number
                ? FederationRating.FromRank( Convert.ToInt32( input.AsNumber ) )
                : FederationRating.FromName( input.AsString ) ?? FederationRating.FromEDName( input.AsString );
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
