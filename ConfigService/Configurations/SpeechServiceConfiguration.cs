using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>
    /// Storage for the Text-to-Speech Configs
    /// </summary>
    [JsonObject( MemberSerialization.OptOut ), RelativePath( @"\speech.json" )]
    public class SpeechServiceConfiguration : Config
    {
        [JsonProperty("standardVoice")]
        public string StandardVoice { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; } = 80;

        [JsonProperty("effectsLevel")]
        public int EffectsLevel { get; set; } = 50;

        [JsonProperty("distortOnDamage")]
        public bool DistortOnDamage { get; set; } = true;

        [JsonProperty("rate")]
        public int Rate { get; set; }

        [JsonProperty("disableipa")]
        public bool DisableIpa { get; set; }

        [JsonProperty("enableicao")]
        public bool EnableIcao { get; set; }

        [JsonProperty("pocketTtsEnabled")]
        public bool PocketTtsEnabled { get; set; }

        [JsonProperty("pocketTtsServerUrl")]
        public string PocketTtsServerUrl { get; set; } = "http://localhost:8000";

        /// <summary>
        /// Clear the information held by speech
        /// </summary>
        public void Clear()
        {
            StandardVoice = null;
            Volume = 100;
            EffectsLevel = 50;
            DistortOnDamage = true;
            DisableIpa = false;
            EnableIcao = false;
            PocketTtsEnabled = false;
            PocketTtsServerUrl = "http://localhost:8000";
        }
    }
}
