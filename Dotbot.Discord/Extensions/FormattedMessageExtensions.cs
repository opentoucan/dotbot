using Discord;
using Dotbot.Common.Models;

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
}