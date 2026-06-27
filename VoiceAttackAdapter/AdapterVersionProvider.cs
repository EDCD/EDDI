#nullable enable

namespace EddiVoiceAttackAdapter
{
    internal static class AdapterVersionProvider
    {
        public static string GetDisplayVersion ( IEddiInstallLocatorStore? store = null )
        {
            return EddiInstallLocator.ResolveVersion( store ) ??
                   typeof( AdapterVersionProvider ).Assembly.GetName().Version?.ToString() ??
                   "unknown";
        }
    }
}
