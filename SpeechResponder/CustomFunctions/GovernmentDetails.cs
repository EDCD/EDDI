using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GovernmentDetails : ICustomFunction
    {
        public string name => "GovernmentDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.GovernmentDetails;
        public Type ReturnType => typeof( Government );
        public IFunction function => Function.CreateNative1( ( runtime, government, writer ) =>
        {
            var result = Government.FromName(government.AsString) ?? 
                                  Government.FromEDName(government.AsString);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
