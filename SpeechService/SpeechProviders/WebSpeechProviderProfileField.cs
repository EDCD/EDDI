namespace EddiSpeechService.SpeechProviders
{
    public sealed class WebSpeechProviderProfileField (
        string key,
        string displayName,
        bool isSecret = false )
    {
        public string Key { get; } = key;

        public string DisplayName { get; } = displayName;

        public bool IsSecret { get; } = isSecret;
    }
}
