using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BigRRadio_DiscordRichPresence
{
    public class StreamInfo
    {
        [JsonPropertyName("stream-hls-url")]
        public string StreamHlsUrl { get; set; }

        [JsonPropertyName("current-track")]
        public Track CurrentTrack { get; set; }
    }
}