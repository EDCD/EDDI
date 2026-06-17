using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechService.SpeechProviders
{
    public sealed class WebSpeechProviderDescriptor (
        string providerType,
        string displayName,
        IEnumerable<WebSpeechProviderProfileField> profileFields,
        string setupUrl = null,
        string accountUrl = null )
    {
        public string ProviderType { get; } = providerType;

        public string DisplayName { get; } = displayName;

        public IReadOnlyList<WebSpeechProviderProfileField> ProfileFields { get; } = profileFields?.ToList() ?? [];

        public string SetupUrl { get; } = setupUrl;

        public string AccountUrl { get; } = accountUrl;

        public override string ToString() => DisplayName;
    }
}
