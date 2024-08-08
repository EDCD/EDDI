using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ExplorationRatingDetails : ICustomFunction
    {
        public string name => "ExplorationRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.ExplorationRatingDetails;
        public Type ReturnType => typeof( ExplorationRating );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var result = input.Type == ValueContent.Number
                ? ExplorationRating.FromRank( Convert.ToInt32( input.AsNumber ) )
                : ExplorationRating.FromName( input.AsString ) ?? ExplorationRating.FromEDName( input.AsString );
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
