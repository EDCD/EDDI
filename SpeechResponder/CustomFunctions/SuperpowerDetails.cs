using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SuperpowerDetails : ICustomFunction
    {
        public string name => "SuperpowerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SuperpowerDetails;
        public Type ReturnType => typeof( Superpower );
        public IFunction function => Function.CreateNative1( ( runtime, superpower, writer ) =>
        {
            var result = Superpower.FromNameOrEdName(superpower.AsString) ?? Superpower.None;
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
