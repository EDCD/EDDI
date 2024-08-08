using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class JumpDetails : ICustomFunction
    {
        public string name => "JumpDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.JumpDetails;
        public Type ReturnType => typeof( JumpDetail );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            if (string.IsNullOrEmpty( input.AsString ) ) { return Value.EmptyMap; }
            var result = EDDI.Instance.CurrentShip?.JumpDetails( input.AsString );
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
