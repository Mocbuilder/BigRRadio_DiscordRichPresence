using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRRadio_DiscordRichPresence
{
    public class Track
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("art")]
        public string Art { get; set; }
    }
}
