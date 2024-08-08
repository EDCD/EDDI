using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SecurityLevelDetails : ICustomFunction
    {
        public string name => "SecurityLevelDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SecurityLevelDetails;
        public Type ReturnType => typeof( SecurityLevel );
        public IFunction function => Function.CreateNative1( ( state, input, writer ) =>
        {
            var result = SecurityLevel.FromName(input.AsString) ?? SecurityLevel.FromEDName(input.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
