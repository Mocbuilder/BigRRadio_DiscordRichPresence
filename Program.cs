using DiscordRPC;
using LibVLCSharp.Shared;
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
                    .SetSize(360, 180)
                    .SetResizable(true)
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

            private static void SetNewStream(string url)
            {
                if (_libVLC == null || _mediaPlayer == null) return;

                _mediaPlayer.Stop();

                if (_media != null)
                {
                    _media.MetaChanged -= OnMetaChanged;
                    _media.Dispose();
                }

                _media = new Media(_libVLC, new Uri(url), ":no-video");
                _media.MetaChanged += OnMetaChanged;

                _mediaPlayer.Play(_media);
            }

            private static void OnMetaChanged(object? sender, MediaMetaChangedEventArgs e)
            {
                if (e.MetadataType == MetadataType.NowPlaying && _media != null && _window != null && _discordClient != null)
                {
                    string rawTitle = _media.Meta(MetadataType.NowPlaying);
                    if (!string.IsNullOrWhiteSpace(rawTitle))
                    {
                        var parts = rawTitle.Split(" - ", 2);
                        string artist = parts.Length > 0 ? parts[0].Trim() : "Big R Radio";
                        string song = parts.Length > 1 ? parts[1].Trim() : rawTitle.Trim();

                        var jsonPayload = JsonSerializer.Serialize(new { artist, song });
                        _window.SendWebMessage(jsonPayload);

                        _discordClient.SetPresence(new RichPresence()
                        {
                            Details = song,
                            State = $"by {artist}",
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
            }
        }
    }
}