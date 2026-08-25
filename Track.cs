using System.Text.Json.Serialization;

namespace BigRRadio_DiscordRichPresence
{
    public class Track
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("art")]
        public string Art { get; set; }
    }
}