using EddiConfigService.Configurations;
using EddiDataDefinitions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EddiSpeechService.SpeechProviders
{
    internal interface IWebSpeechProvider
    {
        string ProviderType { get; }
        string DisplayName { get; }
        WebSpeechProviderDescriptor Descriptor { get; }
        WebSpeechProvider CreateProfile();
        void MigrateLegacyConfiguration ( SpeechServiceConfiguration configuration );
        bool IsConfigured ( WebSpeechProvider profile );
        Task<IReadOnlyList<VoiceDetails>> GetVoicesAsync (
            WebSpeechProvider profile,
            CancellationToken ct );
        Task<Stream> SynthesizeAsync (
            WebSpeechProvider profile,
            VoiceDetails voice,
            string speech,
            SpeechServiceConfiguration configuration,
            CancellationToken ct );
        Task ValidateAsync ( WebSpeechProvider profile, CancellationToken ct );
    }
}
