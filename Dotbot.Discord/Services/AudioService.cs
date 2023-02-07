using System.Collections.Concurrent;
using CliWrap;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Dotbot.Discord.Services;

public class AudioService : IAudioService
{
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels = new();
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _streamCancellationTokens = new();
    private readonly ConcurrentDictionary<ulong, ConcurrentQueue<string>> _audioQueue = new();
    private readonly ConcurrentDictionary<ulong, string> _currentlyPlaying = new();

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;
    }

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        _logger.LogDebug("{}", Environment.CurrentManagedThreadId);
        if (_connectedChannels.TryGetValue(guild.Id, out _))
        {
            return;
        }

        var audioClient = await target.ConnectAsync();

        if (_connectedChannels.TryAdd(guild.Id, audioClient))
        {
            _audioQueue.TryAdd(guild.Id, new ConcurrentQueue<string>());
            // If you add a method to log happenings from this service,
            // you can uncomment these commented lines to make use of that.
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (_connectedChannels.TryRemove(guild.Id, out var client))
        {
            _logger.LogDebug("Leaving VC in {GuildId}", guild.Id);

            await client.StopAsync();
        }
    }

    public async Task EnqueueAudio(IGuild guild, IMessageChannel channel, string url)
    {
        var guid = Guid.NewGuid().ToString();
        await LogToChannel(nameof(EnqueueAudio), guid, "", channel);
        _logger.LogDebug("Add {} to queue for {}", url, guild.Id);
        if (!_audioQueue.ContainsKey(guild.Id))
        {
            await LogToChannel(nameof(EnqueueAudio), guid, "Add new queue for guild", channel);
            _audioQueue.TryAdd(guild.Id, new ConcurrentQueue<string>(new[] { url }));
        }
        else
        {
            _audioQueue[guild.Id].Enqueue(url);
            await LogToChannel(nameof(EnqueueAudio), guid,
                $"Adding to existing queue Size: {_audioQueue[guild.Id].Count}", channel);
        }

        await LogToChannel(nameof(EnqueueAudio), guid,
            $"Is {(_currentlyPlaying.ContainsKey(guild.Id) ? "" : "Not")} currently playing", channel);
        if (!_currentlyPlaying.ContainsKey(guild.Id))
        {
            await SendAudioAsync(guid, guild, channel);
        }
    }

    private static async Task LogToChannel(string source, string guid, string msg, IMessageChannel channel)
    {
        await channel.SendMessageAsync($"<{guid}> {source}(): {msg}");
        Thread.Sleep(50);
    }

    public async Task SendAudioAsync(string guid, IGuild guild, IMessageChannel channel)
    {
        await LogToChannel(nameof(SendAudioAsync), guid, "", channel);
        if (!_audioQueue[guild.Id].TryDequeue(out var url))
        {
            await LogToChannel(nameof(SendAudioAsync), guid, "Nothing Queued", channel);
            _logger.LogDebug("Nothing queued");
            return;
        }

        _currentlyPlaying[guild.Id] = url;

        _logger.LogDebug("Received request to play {Url} in {GuildId}", url, guild.Id);
        await LogToChannel(nameof(SendAudioAsync), guid,
            $"Playing {url.TrimStart('h')} (queue now at {_audioQueue.Count})", channel);

        var offsetInSeconds = 0;
        if (url.Contains("?t")) offsetInSeconds = int.Parse(url.Split("?t=")[1]);
        if (_connectedChannels.TryGetValue(guild.Id, out var client))
        {
            var youtube = new YoutubeClient();

            // You can specify either video ID or URL
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().TryGetWithHighestBitrate();

            if (streamInfo == null)
            {
                _logger.LogError("Failed to get streaminfo for {Url}", url);
                return;
            }

            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            var memoryStream = new MemoryStream();
            var timeSpan = new TimeSpan(0, 0, offsetInSeconds);
            var ffmpegArguments =
                $"-ss {timeSpan:hh\\:mm\\:ss} -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1";
            _logger.LogDebug("Running ffmpeg with arguments {Args}", ffmpegArguments);
            await LogToChannel(nameof(SendAudioAsync), guid, $"Running ffmpeg with arguments {ffmpegArguments}",
                channel);

            if (_streamCancellationTokens.TryGetValue(guild.Id, out var csSource))
            {
                await LogToChannel(nameof(SendAudioAsync), guid, "Have cancellation token", channel);

                if (!csSource.IsCancellationRequested)
                {
                    await LogToChannel(nameof(SendAudioAsync), guid, "Cancelling existing token", channel);

                    csSource.Cancel();
                    csSource.Dispose();
                }
            }

            csSource = new CancellationTokenSource();
            _streamCancellationTokens[guild.Id] = csSource;

            await LogToChannel(nameof(SendAudioAsync), guid, "Creating PCM Stream", channel);
            await using var discord = client.CreatePCMStream(AudioApplication.Mixed);
            try
            {
                await LogToChannel(nameof(SendAudioAsync), guid, "Calling ffpmeg", channel);
                await Cli.Wrap("ffmpeg")
                    .WithArguments(ffmpegArguments)
                    .WithStandardInputPipe(PipeSource.FromStream(stream))
                    .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                    .ExecuteAsync(csSource.Token);
                await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length, csSource.Token);
                await LogToChannel(nameof(SendAudioAsync), guid, "Finished writing to buffer", channel);
            }
            catch (Exception e)
            {
                _logger.LogDebug("{}", e.Message);
                await LogToChannel(nameof(SendAudioAsync), guid, $"{e.Message}", channel);
            }
            finally
            {
                await discord.FlushAsync();
            }

            if (!_audioQueue[guild.Id].IsEmpty)
            {
                await LogToChannel(nameof(SendAudioAsync), guid,
                    $"Queue contains {string.Join(", ", _audioQueue[guild.Id].ToList())}", channel);
                await LogToChannel(nameof(SendAudioAsync), guid, $"Queue is not empty calling {nameof(SendAudioAsync)}",
                    channel);
                await SendAudioAsync(guid, guild, channel);
            }

            _currentlyPlaying.Remove(guild.Id, out _);
            await LogToChannel(nameof(SendAudioAsync), guid,
                $"Marked as not currently playing {_currentlyPlaying.ContainsKey(guild.Id)}", channel);
            await LogToChannel(nameof(SendAudioAsync), guid, $"Exiting", channel);
        }
    }

    public async Task Skip(SocketGuild contextGuild, ISocketMessageChannel contextChannel)
    {
        if (_currentlyPlaying.ContainsKey(contextChannel.Id) &&
            _streamCancellationTokens.TryGetValue(contextGuild.Id, out var csToken))
        {
            csToken.Cancel();
        }

        await SendAudioAsync(Guid.NewGuid().ToString(), contextGuild, contextChannel);
    }
}