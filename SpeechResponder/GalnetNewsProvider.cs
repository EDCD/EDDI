using EddiCore;

namespace EddiSpeechResponder
{
    internal static class GalnetNewsProvider
    {
        internal static IGalnetNewsProvider Instance =>
            EDDI.Instance.ObtainMonitor( "Galnet monitor" ) as IGalnetNewsProvider;
    }
}
