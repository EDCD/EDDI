using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class BlueprintDetails : ICustomFunction
    {
        public string name => "BlueprintDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.BlueprintDetails;
        public Type ReturnType => typeof(Blueprint);

        public IFunction function => Function.CreateNative2( ( runtime, blueprintName, blueprintGrade, writer ) =>
        {
            var result = Blueprint.FromNameAndGrade( blueprintName.AsString,  Convert.ToInt32(blueprintGrade.AsNumber) );
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        } );
    }
}
