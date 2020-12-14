using Cottle.Functions;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class RefreshProfile : ICustomFunction
    {
        public string name => "RefreshProfile";
        // This is a developer tool - FDev have asked us not to abuse their server with too many requests and we respect that request.
        public FunctionCategory Category => FunctionCategory.Hidden;
        public string description => Properties.CustomFunctions_Untranslated.RefreshProfile;
        public NativeFunction function => new NativeFunction((values) =>
        {
            bool stationRefresh = (values.Count != 0 && values[0].AsBoolean);
            EDDI.Instance.refreshProfile(stationRefresh);
            return "";
        }, 0, 1);
    }
}
