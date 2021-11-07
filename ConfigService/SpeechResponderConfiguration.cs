using Newtonsoft.Json;

namespace EddiConfigService
{
    /// <summary>Configuration for the speech responder</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\speechresponder.json")]
    public class SpeechResponderConfiguration : Config
    {
        [JsonProperty("personality")] 
        public string Personality { get; set; } = "EDDI";

        [JsonProperty("subtitles")]
        public bool Subtitles { get; set; }

        [JsonProperty("subtitlesonly")]
        public bool SubtitlesOnly { get; set; }
    }
}
