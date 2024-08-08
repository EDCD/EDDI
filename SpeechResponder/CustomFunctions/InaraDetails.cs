using Cottle;
using EddiInaraService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class InaraDetails : ICustomFunction
    {
        public string name => "InaraDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.InaraDetails;
        public Type ReturnType => typeof( InaraCmdr );
        public IFunction function => Function.CreateNative1( ( runtime, cmdrName, writer ) =>
        {
            var commanderName = cmdrName.AsString;
            if ( !string.IsNullOrWhiteSpace( commanderName ) )
            {
                IInaraService inaraService = new InaraService();
                var result = inaraService.GetCommanderProfile(commanderName);
                return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            return "";
        });
    }
}
