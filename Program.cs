using DiscordRPC;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using Photino.NET;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BigRRadio_DiscordRichPresence
{
    namespace BigRRadioPhotino
    {
        internal class Program
        {
            private static LibVLC? _libVLC;
            private static MediaPlayer? _mediaPlayer;
            private static Media? _media;
            private static DiscordRpcClient? _discordClient;
            private static PhotinoWindow? _window;
            private static readonly HttpClient _httpClient = new HttpClient();
            private static Timer? _metadataTimer;

            public static string CurrentApiUrl { get; private set; } = InitialApiUrl;

            private const string InitialApiUrl = "https://api.live365.com/station/a55004";
            private const string DiscordAppId = "1537901110067470397";

            [STAThread]
            static void Main(string[] args)
            {
                Core.Initialize();

                _libVLC = new LibVLC();
                _mediaPlayer = new MediaPlayer(_libVLC)
                {
                    Volume = 100
                };
                _discordClient = new DiscordRpcClient(DiscordAppId);
                _discordClient.Initialize();

                string tempIconPath = ExtractResourceToTempFile("BigRRadio_DiscordRichPresence.VERT_logo_bigrradio.ico");
                string htmlPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");

                _window = new PhotinoWindow()
                    .SetTitle("Big R Radio")
                    .SetIconFile(tempIconPath)
                    .SetUseOsDefaultSize(false)
                    .SetSize(new System.Drawing.Size(550, 960))
                    .Center()
                    .SetResizable(false)
                    .SetContextMenuEnabled(false)
                    .SetDevToolsEnabled(false)
                    .Load(htmlPath);

                _window.RegisterWebMessageReceivedHandler((sender, message) =>
                {
                    if (_mediaPlayer == null || _discordClient == null) return;

                    ParseMessage(message);

                    /*
                    if (message == "toggle")
                    {
                        if (_mediaPlayer.IsPlaying)
                            _mediaPlayer.Volume = 0;
                        else
                            _mediaPlayer.Volume = 100;
                    }
                    else if (message.StartsWith("vol:"))
                    {
                        if (int.TryParse(message.Substring(4), out int volume))
                        {
                            _mediaPlayer.Volume = volume;
                        }
                    }
                    else if (message.StartsWith("channel:"))
                    {
                        string stationId = message.Substring(8);

                        // Supports both full URLs and raw station IDs (e.g. "a55004")
                        string newApiUrl = stationId.StartsWith("http")
                            ? stationId
                            : $"https://api.live365.com/station/{stationId}";

                        _ = SetNewStreamAsync(newApiUrl);
                    }
                    */
                });

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    await SetNewStreamAsync(InitialApiUrl);
                });

                _metadataTimer = new Timer(async _ => await RefreshMetadataAsync(), null, 15000, 15000);

                _window.WaitForClose();

                _metadataTimer.Dispose();
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _media?.Dispose();
                _libVLC.Dispose();
                _discordClient.Dispose();
                _httpClient.Dispose();

                string appTempDir = Path.Combine(Path.GetTempPath(), "BigRRadio_RPC");
                if (Directory.Exists(appTempDir))
                {
                    try { Directory.Delete(appTempDir, true); }
                    catch { /* Ignore lock issues */ }
                }
            }

            private static void ParseMessage(string message)
            {
                switch (message)
                {
                    case "toggle":
                        _mediaPlayer.Volume = _mediaPlayer.IsPlaying ? 0 : 100;
                        break;

                    case string s when s.StartsWith("vol:"):
                        if (int.TryParse(s.Substring(4), out int volume))
                        {
                            _mediaPlayer.Volume = volume;
                        }
                        break;

                    case string s when s.StartsWith("channel:"):
                        string stationId = s.Substring(8);
                        string newApiUrl = stationId.StartsWith("http")
                            ? stationId
                            : $"https://api.live365.com/station/{stationId}";

                        _ = SetNewStreamAsync(newApiUrl);
                        break;
                    case "GetStations":
                        _window.SendWebMessage(GetJsonEmbeddedResource("stations.json"));
                        break;
                }
            }

            private static string GetJsonEmbeddedResource(string filename)
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "BigRRadio_DiscordRichPresence." + filename;

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException($"Could not find embedded resource: {resourceName}");

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            private static string ExtractResourceToTempFile(string resourceName)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream? stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }

                string appTempDir = Path.Combine(Path.GetTempPath(), "BigRRadio_RPC");
                Directory.CreateDirectory(appTempDir);

                string extension = Path.GetExtension(resourceName);
                string tempFilePath = Path.Combine(appTempDir, $"app_icon{extension}");

                using FileStream fileStream = File.Create(tempFilePath);
                stream.CopyTo(fileStream);

                return tempFilePath;
            }

            private static async Task SetNewStreamAsync(string apiUrl)
            {
                CurrentApiUrl = apiUrl;

                StreamInfo? streamInfo = await GetStreamInfoAsync(CurrentApiUrl);
                if (streamInfo == null || string.IsNullOrEmpty(streamInfo.StreamHlsUrl))
                {
                    Console.WriteLine("Failed to parse station info or missing audio stream URL.");
                    return;
                }

                _mediaPlayer?.Stop();
                _media?.Dispose();

                _media = new Media(_libVLC, new Uri(streamInfo.StreamHlsUrl), ":no-video");
                _mediaPlayer?.Play(_media);

                UpdatePresenceAndUI(streamInfo);
            }

            private static async Task RefreshMetadataAsync()
            {
                try
                {
                    StreamInfo? streamInfo = await GetStreamInfoAsync(CurrentApiUrl);
                    if (streamInfo != null)
                    {
                        UpdatePresenceAndUI(streamInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Metadata refresh failed: {ex.Message}");
                }
            }

            private static void UpdatePresenceAndUI(StreamInfo streamInfo)
            {
                if (_window != null)
                {
                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(streamInfo);
                    _window.SendWebMessage(jsonPayload);
                }

                if (_discordClient != null)
                {
                    _discordClient.SetPresence(new RichPresence()
                    {
                        Details = streamInfo.CurrentTrack?.Title ?? "Unknown",
                        State = $"by {streamInfo.CurrentTrack?.Artist ?? "Unknown"}",
                        Type = ActivityType.Listening,
                        Assets = new Assets()
                        {
                            LargeImageKey = "radio_logo",
                            LargeImageText = "Big R Radio"
                        },
                        Timestamps = Timestamps.Now
                    });
                }
            }

            public static async Task<StreamInfo?> GetStreamInfoAsync(string url)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        return System.Text.Json.JsonSerializer.Deserialize<StreamInfo>(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Network error: {ex.Message}");
                }

                return null;
            }
        }
    }
}