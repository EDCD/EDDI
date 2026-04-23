
using Eddi.BuildSecrets;

namespace EddiCompanionAppService
{
    public class ClientId
    {
        // replace with official FDev Client ID. We don't need the Client Secret for PKCE authentication
        internal static string ID =>
            string.IsNullOrWhiteSpace( BuildInjectedSecrets.CompanionAppClientId )
                ? null
                : BuildInjectedSecrets.CompanionAppClientId;
    }
}
