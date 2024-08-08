using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CombatRatingDetails : ICustomFunction
    {
        public string name => "CombatRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CombatRatingDetails;
        public Type ReturnType => typeof( CombatRating );
        public IFunction function => Function.CreateNative1( ( runtime, combatRank, writer ) =>
        {
            var result = combatRank.Type == ValueContent.Number 
                ? CombatRating.FromRank( Convert.ToInt32( combatRank.AsNumber) ) 
                : CombatRating.FromName(combatRank.AsString) ?? CombatRating.FromEDName(combatRank.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
