using System.Text.Json.Serialization;

namespace BigRRadio_DiscordRichPresence
{
    public class StreamInfo
    {
        [JsonPropertyName("stream-url")]
        public string StreamUrl { get; set; }

        [JsonPropertyName("current-track")]
        public Track CurrentTrack { get; set; }
    }
}