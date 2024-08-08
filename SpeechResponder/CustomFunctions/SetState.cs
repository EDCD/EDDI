using Cottle;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SetState :  ICustomFunction
    {
        public string name => "SetState";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SetState;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative2((runtime, variableName, variableValue, writer) =>
        {
            var varName = variableName.AsString.ToLowerInvariant().Replace(" ", "_");
            var value = variableValue;
            if (value.Type == ValueContent.Boolean)
            {
                EDDI.Instance.State[varName] = value.AsBoolean;
            }
            else if (value.Type == ValueContent.Number)
            {
                EDDI.Instance.State[ varName ] = Convert.ToDecimal( value.AsNumber );
            }
            else if (value.Type == ValueContent.String)
            {
                EDDI.Instance.State[varName] = value.AsString;
            }
            else if (value.Type == ValueContent.Void)
            {
                EDDI.Instance.State[varName] = null;
            }
            // Ignore other possibilities
            return "";
        });
    }
}
