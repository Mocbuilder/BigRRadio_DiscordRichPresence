using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BigRRadio_DiscordRichPresence
{
    public class Station
    {
        [JsonPropertyName("id")]
        string ID { get; set; }

        [JsonPropertyName("name")]
        string Name { get; set; }
    }
}
