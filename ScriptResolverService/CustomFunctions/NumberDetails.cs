using Cottle;
using EddiSpeechService.SpeechConversions;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class NumberDetails : ICustomFunction
    {
        public string name => "NumberDetails";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.NumberDetails;
        public Type ReturnType => typeof( SpeechConversions.HumanizedNumber );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var number = input.Type == ValueContent.Number
                ? (decimal?)Convert.ToDecimal( input.AsNumber )
                : null;
            var result = SpeechConversions.DecomposeHumanizedNumber( number );
            return result is null
                ? Value.EmptyMap
                : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public );
        } );
    }
}
