namespace Dotbot.Database.Entities;

public class Meme: Entity
{
    public enum MediaType
    {
        Image, Link
    }

    public required string MessageId { get; set; }
    public required string ServerId { get; set; }
    public required string UserId { get; set; }
    public required string Url { get; set; }
    public MediaType UrlType { get; set; } = MediaType.Image;
    
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    
    public List<string> Upvoters { get; set; } = new();
    public List<string> Downvoters { get; set; } = new();

    public int Score => Upvoters.Count - Downvoters.Count;
    public int Downvotes => Downvoters.Count;
    public int Upvotes => Upvoters.Count;

}
