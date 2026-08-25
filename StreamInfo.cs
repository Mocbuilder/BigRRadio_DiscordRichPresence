using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BigRRadio_DiscordRichPresence
{
    public class StreamInfo
    {
        [JsonProperty("stream-hls-url")]
        [JsonPropertyName("stream-hls-url")]
        public string StreamHlsUrl { get; set; }

        [JsonProperty("current-track")]
        [JsonPropertyName("current-track")]
        public Track CurrentTrack { get; set; }
    }
}