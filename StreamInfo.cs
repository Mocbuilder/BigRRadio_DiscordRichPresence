using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BigRRadio_DiscordRichPresence
{
    public class StreamInfo
    {
        [JsonProperty("stream_hls-url")]
        public string StreamHlsUrl { get; set; }
        [JsonProperty("current-track")]
        public Track CurrentTrack { get; set; }
    }
}
