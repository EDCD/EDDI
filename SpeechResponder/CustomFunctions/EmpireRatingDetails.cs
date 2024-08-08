using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EmpireRatingDetails : ICustomFunction
    {
        public string name => "EmpireRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EmpireRatingDetails;
        public Type ReturnType => typeof( EmpireRating );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var result = input.Type == ValueContent.Number 
                ? EmpireRating.FromRank(Convert.ToInt32(input.AsNumber)) 
                : EmpireRating.FromName(input.AsString) ?? EmpireRating.FromEDName(input.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
