using System.Collections.Concurrent;
using Discord.Audio;
using YoutubeExplode.Videos;

namespace Discord.Application.Models;

public class AudioState
{
    public readonly Guid Id = Guid.NewGuid();
    public IAudioClient? Client { get; set; }
    public CancellationTokenSource? CancellationToken { get; set; }
    public TrackInfo? CurrentlyPlaying { get; set; }
    public ConcurrentQueue<string> AudioQueue { get; } = new();

    public class TrackInfo
    {
        public string? Url { get; set; }
        public string? Title { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Platform { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Author { get; set; }

        public TrackInfo()
        {
        }

        public TrackInfo(Video video)
        {
            Url = video.Url;
            Title = video.Title;
            Duration = video.Duration;
            Platform = "YouTube";
            Author = video.Author.ChannelTitle;
            if (video.Thumbnails.Any())
            {
                ThumbnailUrl = video.Thumbnails[0].Url;
            }
        }
    }
}