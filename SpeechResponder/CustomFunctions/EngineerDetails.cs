using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EngineerDetails : ICustomFunction
    {
        public string name => "EngineerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.EngineerDetails;
        public Type ReturnType => typeof( Engineer );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            Engineer result = Engineer.FromName(input.AsString) ?? Engineer.FromSystemName(input.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
