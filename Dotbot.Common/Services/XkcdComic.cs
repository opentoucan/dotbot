using System.Text.Json.Serialization;

namespace Dotbot.Common.Services;

public class XkcdComic
{
    public int Month { get; set; }
    public int Num { get; set; }
    public string Link { get; set; }
    public string Year { get; set; }
    public string News { get; set; }
    [JsonPropertyName("safe_title")]
    public string SafeTitle { get; set; }
    public string Transcript { get; set; }
    public string Alt { get; set; }
    public string Img { get; set; }
    public string Title { get; set; }
    public string Day { get; set; }
}