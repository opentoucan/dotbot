using Discord;
using Discord.Commands;
using Discord.Interactions;
using Dotbot.Discord.Services;
using RunMode = Discord.Interactions.RunMode;

namespace Dotbot.Discord.Modules;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private InteractionHandler.InteractionHandler _handler;
        private readonly IAudioService _audioService;

        // Constructor injection is also a valid way to access the dependencies
        public InteractionModule(InteractionHandler.InteractionHandler handler, IAudioService audioService)
        {
            _handler = handler;
            _audioService = audioService;
        }

        // You can use a number of parameter types in you Slash Command handlers (string, int, double, bool, IUser, IChannel, IMentionable, IRole, Enums) by default. Optionally,
        // you can implement your own TypeConverters to support a wider range of parameter types. For more information, refer to the library documentation.
        // Optional method parameters(parameters with a default value) also will be displayed as optional on Discord.

        // [Summary] lets you customize the name and the description of a parameter
        [SlashCommand("echo", "Repeat the input")]
        public async Task Echo(string echo, [global::Discord.Interactions.Summary(description: "mention the user")] bool mention = false)
            => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

        [SlashCommand("bitrate", "Gets the bitrate of a specific voice channel.")]
        public async Task GetBitrateAsync([ChannelTypes(ChannelType.Voice, ChannelType.Stage)] IChannel channel)
            => await RespondAsync(text: $"This voice channel has a bitrate of {(channel as IVoiceChannel).Bitrate}");

        [SlashCommand("play", "play music",false, RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await _audioService.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            await RespondAsync($"Playing audio");
            await _audioService.EnqueueAudio(Context.Guild, Context.Channel, song);
        }
        
        [SlashCommand("skip", "Skip current track",false, RunMode.Async)]
        public async Task SkipCmd()
        {
            await _audioService.Skip(Context.Guild, Context.Channel);
        }
        
        // Use [ComponentInteraction] to handle message component interactions. Message component interaction with the matching customId will be executed.
        // Alternatively, you can create a wild card pattern using the '*' character. Interaction Service will perform a lazy regex search and capture the matching strings.
        // You can then access these capture groups from the method parameters, in the order they were captured. Using the wild card pattern, you can cherry pick component interactions.
        [ComponentInteraction("musicSelect:*,*")]
        public async Task ButtonPress(string id, string name)
        {
            // ...
            await RespondAsync($"Playing song: {name}/{id}");
        }

        // Select Menu interactions, contain ids of the menu options that were selected by the user. You can access the option ids from the method parameters.
        // You can also use the wild card pattern with Select Menus, in that case, the wild card captures will be passed on to the method first, followed by the option ids.
        [ComponentInteraction("roleSelect")]
        public async Task RoleSelect(string[] selections)
        {
            throw new NotImplementedException();
        }

        // With the Attribute DoUserCheck you can make sure that only the user this button targets can click it. This is defined by the first wildcard: *.
        // See Attributes/DoUserCheckAttribute.cs for elaboration.
        [ComponentInteraction("myButton:*")]
        public async Task ClickButtonAsync(string userId)
            => await RespondAsync(text: ":thumbsup: Clicked!");

        // This command will greet target user in the channel this was executed in.
        [UserCommand("greet")]
        public async Task GreetUserAsync(IUser user)
            => await RespondAsync(text: $":wave: {Context.User} said hi to you, <@{user.Id}>!");

        // Pins a message in the channel it is in.
        [MessageCommand("pin")]
        public async Task PinMessageAsync(IMessage message)
        {
            // make a safety cast to check if the message is ISystem- or IUserMessage
            if (message is not IUserMessage userMessage)
                await RespondAsync(text: ":x: You cant pin system messages!");

            // if the pins in this channel are equal to or above 50, no more messages can be pinned.
            else if ((await Context.Channel.GetPinnedMessagesAsync()).Count >= 50)
                await RespondAsync(text: ":x: You cant pin any more messages, the max has already been reached in this channel!");

            else
            {
                await userMessage.PinAsync();
                await RespondAsync(":white_check_mark: Successfully pinned message!");
            }
        }
}