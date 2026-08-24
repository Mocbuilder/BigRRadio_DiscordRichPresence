using DiscordRPC;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using Photino.NET;
using System;
using System.Text.Json;

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

                SetNewStream(InitialStreamUrl);

                _window.RegisterWebMessageReceivedHandler((sender, message) =>
                {
                    if (_mediaPlayer == null || _discordClient == null) return;

                    if (message == "toggle")
                    {
                        if (_mediaPlayer.IsPlaying)
                        {
                            _mediaPlayer.Pause();
                        }
                        else
                        {
                            _mediaPlayer.Play();
                        }
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

                _mediaPlayer.Play(_media);

                _window.WaitForClose();

                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _media?.Dispose();
                _libVLC.Dispose();
                _discordClient.Dispose();
            }

            private static StreamInfo? SetNewStream(string url)
            {
                CurrentStreamUrl = url;
                StreamInfo streamInfo = GetStreamInfo(url);
                if (streamInfo == null || streamInfo.CurrentTrack == null)
                {
                    Console.WriteLine("Failed to retrieve stream info or current track.");
                    return null;
                }

                _mediaPlayer?.Stop();

                if (_media != null)
                {
                    _media.MetaChanged -= OnMetaChanged;
                    _media.Dispose();
                }

                _media = new Media(_libVLC, new Uri(streamInfo.StreamHlsUrl), ":no-video");
                _media.MetaChanged += OnMetaChanged;

                _mediaPlayer?.Play(_media);
                return streamInfo;
            }

            private static void OnMetaChanged(object? sender, MediaMetaChangedEventArgs e)
            {
                if (_media != null && _window != null && _discordClient != null)
                {
                    StreamInfo? streamInfo = SetNewStream(CurrentStreamUrl);
                    if (streamInfo == null) return;

                    var jsonPayload = System.Text.Json.JsonSerializer.Serialize(streamInfo);
                    _window.SendWebMessage(jsonPayload);

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

            public static StreamInfo GetStreamInfo(string url)
            {
                using HttpClient client = new HttpClient();
                var response = client.GetAsync(url).Result;
                StreamInfo? streamInfo;
                var jsonString = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    jsonString = response.Content.ReadAsStringAsync().Result;
                    streamInfo = JsonConvert.DeserializeObject<StreamInfo>(jsonString);
                    if (streamInfo != null && streamInfo.CurrentTrack != null)
                    {
                        return streamInfo;
                    }
                    else
                    {
                        Console.WriteLine($"StreamInfo or CurrentTrack is null.\n{jsonString}");
                        Console.ReadKey();
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"No Success Status Code.\n{response.StatusCode}");
                    Console.ReadKey();
                    return null;
                }
            }
        }
    }
}