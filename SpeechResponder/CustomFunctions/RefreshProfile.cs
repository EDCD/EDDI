using Cottle;
using EddiCore;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Threading.Tasks;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class RefreshProfile : ICustomFunction
    {
        public string name => "RefreshProfile";
        // This is a developer tool - FDev have asked us not to abuse their server with too many requests and we respect that request.
        public FunctionCategory Category => FunctionCategory.Hidden;
        public string description => Properties.CustomFunctions_Untranslated.RefreshProfile;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePureMinMax( ( runtime, values ) =>
        {
            var stationRefresh = (values.Count != 0 && values[0].AsBoolean);
            Task.Run( () => EDDI.Instance.refreshProfileAsync( stationRefresh ) ).ConfigureAwait( false );
            return "";
        }, 0, 1);
    }
}
