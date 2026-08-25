using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BigRRadio_DiscordRichPresence
{
    public class Track
    {
        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonProperty("art")]
        [JsonPropertyName("art")]
        public string Art { get; set; }
    }
}