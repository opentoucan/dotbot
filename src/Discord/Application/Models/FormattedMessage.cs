using FluentResults;

namespace Discord.Application.Models;

public class FormattedMessage
{
    public string Title { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string?ImageUrl { get; set; }
    public string? Description { get; set; }
    public System.Drawing.Color? Color { get; set; }
    public List<Field> Fields { get; private init; } = new();

    //TODO: Footer, Author, Header

    public class Field
    {
        public string Name { get; set; }
        public string? Value { get; set; }
        public bool Inline { get; set; } = false;
    }

    private FormattedMessage()
    {
        
    }
    
    public FormattedMessage SetTitle(string title)
    {
        Title = title;
        return this;
    }

    public FormattedMessage SetImage(string url)
    {
        ImageUrl = url;
        return this;
    }

    public FormattedMessage SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public FormattedMessage AppendDescription(string description)
    {
        Description += $"\n{description}";
        return this;
    }

    public FormattedMessage AddField(string name, string value, bool inline = false)
    {
        Fields.Add(new Field
        {
            Name = name,
            Value = value.Length > 1024 ? value[..1023] : value ,
            Inline = inline
        });
        return this;
    }

    public FormattedMessage SetColor(System.Drawing.Color color)
    {
        Color = color;
        return this;
    }
    

    
    public static FormattedMessage Info(string? message = null)
    {
        return new FormattedMessage
        {
            Color = System.Drawing.Color.Blue,
            Title = "Info",
            Description = message
        };
    }
    
    public static FormattedMessage Success(string message)
    {
        return new FormattedMessage
        {
            Color = System.Drawing.Color.LimeGreen,
            Title = "Success",
            Description = message
        };
    }
    
    public static FormattedMessage Error(string? message = null)
    {
        return new FormattedMessage
        {
            Color = System.Drawing.Color.DarkRed,
            Title = "Error",
            Description = message
        };
    }
    
    public static FormattedMessage Error(IEnumerable<IError> errors)
    {
        var errorNum = 1;
        return new FormattedMessage
        {
            Color = System.Drawing.Color.DarkRed,
            Title = "Error",
            Fields = errors.Select(x => new Field{Name = $"Error {errorNum++}", Value = x.Message}).ToList()
        };
    }

    public static FormattedMessage XkcdMessage(XkcdComic comic, bool latest=false)
    {
        return new FormattedMessage
        {
            ImageUrl = comic.ImageUrl,
            Title = latest ? $"Latest Comic #{comic.ComicNumber}" : $"XKCD: #{comic.ComicNumber}",
            Color = System.Drawing.Color.FromArgb(157, 3, 252),
            Fields = new List<Field>
            {
                new()
                {
                    Name = "Title",
                    Value = comic.Title,
                    Inline = true
                },
                new()
                {
                    Name = "Published",
                    Value = $"{comic.DatePosted.DateTime.ToShortDateString()}",
                    Inline = true
                },
                new()
                {
                    Name = "Alt Text",
                    Value = comic.AltText,
                    Inline = true
                }
            }
        };
    }


}