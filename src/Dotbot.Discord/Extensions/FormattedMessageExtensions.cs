using Discord;
using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using YoutubeExplode.Videos;

namespace Dotbot.Discord.Extensions;

public static class FormattedMessageExtensions
{
    public static Embed Convert(this FormattedMessage message)
    {
        Color? color = null;

        if (message.Color != null)
        {
            var msgColor = (System.Drawing.Color)message.Color;
            color = new Color(msgColor.R, msgColor.G, msgColor.B);
        }

        var eb = new EmbedBuilder
        {
            Title = message.Title,
            ImageUrl = message.ImageUrl,
            Description = message.Description,
            Color = color,
            Timestamp = message.Timestamp
        };

        foreach (var field in message.Fields)
        {
            eb.AddField(field.Name, field.Value, field.Inline);
        }

        return eb.Build();
    }

    public static FormattedMessage Youtube(AudioState.TrackInfo trackInfo)
    {
        var fm = FormattedMessage.Info();
        if (trackInfo.Platform != null && trackInfo.Platform.Equals("youtube", StringComparison.InvariantCultureIgnoreCase))
            fm.Color = Color.DarkRed;
        fm.SetTitle(trackInfo.Title);
        fm.AddField("Duration", $"{trackInfo.Duration:g}");
        fm.AddField("Url", trackInfo.Url);
        fm.AddField("Author", trackInfo.Author);
        fm.SetImage(trackInfo.ThumbnailUrl);
        return fm;
    }
    
}