using Contracts.MessageBus;
using Discord;
using Dotbot.Gateway.HttpClient;
using MassTransit;
using Xkcd.Sdk;

namespace Dotbot.Gateway.Application.IntegrationEvents.EventHandlers;

public class XkcdPostedEventHandler(IDiscordWebhookClientFactory discordWebhookClientFactory, ILogger<XkcdPostedEventHandler> logger)
    : IConsumer<XkcdPostedEvent>
{
    public async Task Consume(ConsumeContext<XkcdPostedEvent> context)
    {
        var xkcdComic = new XkcdComic
        {
            AltText = context.Message.AltText,
            ComicNumber = context.Message.ComicNumber,
            DatePosted = context.Message.DatePosted,
            ImageUrl = context.Message.ImageUrl,
            Title = context.Message.Title
        };

        var xkcdWebhook = discordWebhookClientFactory.Create("xkcd");
            
        var titleFields = new EmbedFieldBuilder();
        titleFields.WithName("Title").WithValue(xkcdComic.Title).WithIsInline(true).Build();
        var publishedFields = new EmbedFieldBuilder();
        publishedFields.WithName("Published").WithValue($"{xkcdComic.DatePosted.ToShortDateString()}").WithIsInline(true).Build();
        var altTextFields = new EmbedFieldBuilder();
        altTextFields.WithName("Alt Text").WithValue(xkcdComic.AltText).WithIsInline(true).Build();
            
        var embed = new EmbedBuilder()
            .WithTitle($"Latest Comic #{xkcdComic.ComicNumber}")
            .WithImageUrl(xkcdComic.ImageUrl)
            .WithColor(new Color(157, 3, 252))
            .WithFields(titleFields, publishedFields, altTextFields)
            .Build();

        await xkcdWebhook.SendMessageAsync(embeds: new List<Embed> { embed });
        
        logger.LogInformation(
            "XKCD posted event (message id): {id} has been sent to Discord with comic number: {comicNumber} posted at date: {datePosted}",
            context.MessageId,
            context.Message.ComicNumber,
            context.Message.DatePosted);
        
    }
}