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
        if (_connectedChannels.TryGetValue(guild.Id, out _) && target.Guild.Id != guild.Id)
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
        _logger.LogDebug("Add {} to queue for {}", url, guild.Id);
        if (!_audioQueue.ContainsKey(guild.Id))
        {
            _audioQueue.TryAdd(guild.Id, new ConcurrentQueue<string> (new []{url}));
        }
        else
        {
            _audioQueue[guild.Id].Enqueue(url);
        }

        if (!_currentlyPlaying.ContainsKey(guild.Id))
        {
            await SendAudioAsync(guild, channel);
        }
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel)
    {
        
        if(!_audioQueue[guild.Id].TryDequeue(out var url))
        {
            _logger.LogDebug("Nothing queued");
            return;
        }
        
        _logger.LogDebug("Received request to play {Url} in {GuildId}", url, guild.Id);
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

            if (_streamCancellationTokens.TryGetValue(guild.Id, out var csSource))
            {
                if (!csSource.IsCancellationRequested)
                {
                    csSource.Cancel();
                    csSource.Dispose();
                }
            }

            csSource = new CancellationTokenSource();

            _currentlyPlaying[guild.Id] = url;
            
            await using var discord = client.CreatePCMStream(AudioApplication.Mixed);
            try
            {
                await Cli.Wrap("ffmpeg")
                    .WithArguments(ffmpegArguments)
                    .WithStandardInputPipe(PipeSource.FromStream(stream))
                    .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                    .ExecuteAsync(csSource.Token);
                await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length, csSource.Token);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogDebug("{}",e.Message);
            }
            finally
            {
                await discord.FlushAsync();
            }

            if (!_audioQueue.IsEmpty)
            {
                await SendAudioAsync(guild, channel);
            }
        }
    }

    public async Task Skip(SocketGuild contextGuild, ISocketMessageChannel contextChannel)
    {
        if (_currentlyPlaying.ContainsKey(contextChannel.Id) &&
            _streamCancellationTokens.TryGetValue(contextGuild.Id, out var csToken))
        {
            csToken.Cancel();
        }

        await SendAudioAsync(contextGuild, contextChannel);
    }
}