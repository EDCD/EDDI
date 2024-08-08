using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class TradeRatingDetails : ICustomFunction
    {
        public string name => "TradeRatingDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.TradeRatingDetails;
        public Type ReturnType => typeof( TradeRating );
        public IFunction function => Function.CreateNative1( ( runtime, tradeRating, writer ) =>
        {
            var result = tradeRating.Type == ValueContent.Number 
                ? TradeRating.FromRank(Convert.ToInt32(tradeRating.AsNumber)) 
                : TradeRating.FromName(tradeRating.AsString) ?? TradeRating.FromEDName(tradeRating.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
