using DiscordRPC;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using Photino.NET;
using System;
using System.Net.Http;
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

            public static string CurrentStreamUrl { get; private set; } = InitialStreamUrl;

            private const string InitialStreamUrl = "http://bigrradio.cdnstream1.com/5186_128";
            private const string DiscordAppId = "1537901110067470397";

            [STAThread]
            static void Main(string[] args)
            {
                Core.Initialize();
                _libVLC = new LibVLC();
                _mediaPlayer = new MediaPlayer(_libVLC);
                _discordClient = new DiscordRpcClient(DiscordAppId);
                _discordClient.Initialize();

                _window = new PhotinoWindow()
                    .SetTitle("Big R Radio")
                    .SetUseOsDefaultSize(false)
                    .SetSize(new System.Drawing.Size(600, 905))
                    .Center()
                    .SetResizable(false)
                    .Load("wwwroot/index.html");

                _window.RegisterWebMessageReceivedHandler((sender, message) =>
                {
                    if (_mediaPlayer == null || _discordClient == null) return;

                    if (message == "toggle")
                    {
                        if (_mediaPlayer.IsPlaying)
                            _mediaPlayer.Pause();
                        else
                            _mediaPlayer.Play();
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
                        string streamId = message.Substring(8);
                        string newUrl = $"http://bigrradio.cdnstream1.com/{streamId}";
                        SetNewStream(newUrl);
                    }
                });

                // Load initial stream asynchronously without blocking window creation
                SetNewStream(InitialStreamUrl);

                // Blocks thread until window closes (UI loop)
                _window.WaitForClose();

                // Cleanup
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _media?.Dispose();
                _libVLC.Dispose();
                _discordClient.Dispose();
                _httpClient.Dispose();
            }

            private static void SetNewStream(string url)
            {
                CurrentStreamUrl = url;

                _mediaPlayer?.Stop();

                if (_media != null)
                {
                    _media.MetaChanged -= OnMetaChanged;
                    _media.Dispose();
                }

                // Pass the stream URL directly to LibVLC
                _media = new Media(_libVLC, new Uri(url), ":no-video");
                _media.MetaChanged += OnMetaChanged;

                _mediaPlayer?.Play(_media);
            }

            private static async void OnMetaChanged(object? sender, MediaMetaChangedEventArgs e)
            {
                // Process metadata update asynchronously without calling SetNewStream again
                StreamInfo? streamInfo = await GetStreamInfoAsync(CurrentStreamUrl);
                if (streamInfo != null)
                {
                    UpdatePresenceAndUI(streamInfo);
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
                        return JsonConvert.DeserializeObject<StreamInfo>(jsonString);
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