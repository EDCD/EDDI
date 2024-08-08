using Cottle;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;


namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class GetState : ICustomFunction
    {
        public string name => "GetState";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.GetState;
        public Type ReturnType => typeof( Value );
        public IFunction function => Function.CreateNative1((runtime, variableName, writer) =>
        {
            var varName = variableName.AsString.ToLowerInvariant().Replace(" ", "_");
            if ( EDDI.Instance.State.TryGetValue( varName, out var @value )  )
            {
                if ( value is null )
                {
                    return "";
                }

                if ( @value is bool b )
                {
                    return b;
                }

                if ( @value is decimal d )
                {
                    return d;
                }

                if ( @value is int i )
                {
                    return i;
                }

                if ( @value is string s )
                {
                    return s;
                }

                return $"State variable {varName} does not match any expected type.";
            }
            return "";
        });
    }
}
