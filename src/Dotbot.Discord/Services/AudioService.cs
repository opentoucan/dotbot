using System.Collections.Concurrent;
using CliWrap;
using Discord;
using Discord.Audio;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Dotbot.Discord.Services;

public class AudioState
{
    public readonly Guid Id = Guid.NewGuid();
    public IAudioClient? Client { get; set; }
    public CancellationTokenSource? CancellationToken { get; set; }
    public string? CurrentlyPlaying { get; set; }
    public ConcurrentQueue<string> AudioQueue { get; } = new();
}

public class AudioService : IAudioService
{
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<ulong, AudioState> _audioStates = new();
    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;
    }

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        if (_audioStates.TryGetValue(guild.Id, out var audioState) &&
            audioState.Client is { ConnectionState: ConnectionState.Connected })
        {
            _logger.LogInformation("Already connected {}", guild.Id);
            return;
        }

        _audioStates.TryRemove(guild.Id, out _);
        
        var audioClient = await target.ConnectAsync();

        audioClient.Disconnected += (exception => AudioClientOnDisconnected(guild.Id, exception));

        _audioStates.TryAdd(guild.Id, new AudioState
        {
            Client = audioClient,
        });
    }

    private async Task AudioClientOnDisconnected(ulong id, Exception arg)
    {
        if (_audioStates.TryGetValue(id, out var audioState))
        {
            audioState.CancellationToken?.Cancel();
            audioState.CancellationToken?.Dispose();
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (_audioStates.TryRemove(guild.Id, out var audioState))
        {
            _logger.LogDebug("Leaving VC in {GuildId}", guild.Id);

            await audioState.Client.StopAsync();
        }
    }

    public async Task EnqueueAudioThread(IGuild guild, IVoiceChannel voiceChannel, IMessageChannel channel, string url)
    {
        //This must be run in a new thread otherwise it might block the main thread and cause all sorts of headeaches
        #pragma warning disable CS4014
        Task.Run(() => EnqueueAudio(guild, voiceChannel, channel, url));
        #pragma warning restore CS4014
    }

    private async Task EnqueueAudio(IGuild guild, IVoiceChannel voiceChannel, IMessageChannel channel, string url)
    {
        await JoinAudio(guild, voiceChannel);

        var guid = Guid.NewGuid().ToString();
        _logger.LogDebug("Add {} to queue for {}", url, guild.Id);

        var audioState = _audioStates[guild.Id];

        audioState.AudioQueue.Enqueue(url);
        
        if (audioState.CurrentlyPlaying == null)
        {
            await SendAudioAsync(guid, guild, channel);
        }
    }
    public async Task SendAudioAsync(string guid, IGuild guild, IMessageChannel channel)
    {
        var audioState = _audioStates[guild.Id];

        if (audioState.Client is { ConnectionState: ConnectionState.Disconnected }) return;

        if (!audioState.AudioQueue.TryDequeue(out var url))
        {
            _logger.LogDebug("Nothing queued");
            return;
        }
        _logger.LogDebug("Received request to play {Url} in {GuildId}", url, guild.Id);

        var offsetInSeconds = 0;
        if (url.Contains("?t")) offsetInSeconds = int.Parse(url.Split("?t=")[1]);

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

        if (audioState.CancellationToken is { IsCancellationRequested: false })
        {
            _logger.LogInformation("Cancellation token for instance {}", audioState.Id);
            audioState.CancellationToken.Cancel();
            audioState.CancellationToken.Dispose();
        }

        audioState.CancellationToken = new CancellationTokenSource();

        _logger.LogDebug("Creating PCM Stream for instances {}", audioState.Id);
        await using var discord = audioState.Client.CreatePCMStream(AudioApplication.Mixed);
        audioState.CurrentlyPlaying = url;
        try
        {
            _logger.LogDebug("Calling ffpmeg for instances {}", audioState.Id);
            await Cli.Wrap("ffmpeg")
                .WithArguments(ffmpegArguments)
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync(audioState.CancellationToken.Token);
            //TODO: Send bits of a stream at a time to support big videos
            await discord.WriteAsync(memoryStream.ToArray().AsMemory(0, (int)memoryStream.Length), audioState.CancellationToken.Token);
            _logger.LogDebug("Finished audio stream for instance {}", audioState.Id);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception in sending audio stream: {}", e.Message);
        }
        finally
        {
            audioState.CurrentlyPlaying = null;
            await discord.FlushAsync();
        }

        if (!audioState.AudioQueue.IsEmpty)
        {
            _logger.LogDebug("Queue contains {} for instance {}",string.Join(", ", audioState.AudioQueue.ToList()), audioState.Id);
            _logger.LogDebug("Queue is not empty calling {} for instance {}", nameof(SendAudioAsync), audioState.Id);
            await SendAudioAsync(guid, guild, channel);
        }

        audioState.CurrentlyPlaying = null;
        _logger.LogDebug("Marked as not currently playing for instances {}", audioState.Id);
    }

    public async Task Skip(IGuild contextGuild, IMessageChannel contextChannel)
    {
        var audioState = _audioStates[contextGuild.Id];
        if (audioState is { CurrentlyPlaying: { }, CancellationToken: { } })
        {
            audioState.CancellationToken.Cancel();
        }

        await SendAudioAsync(Guid.NewGuid().ToString(), contextGuild, contextChannel);
    }
}