namespace Discord.Entities;

public class DiscordServer : Entity
{
    public string Server { get; set; }
    
    public Dictionary<string, int> UserWordCounts { get; set; } = new();
    
    public List<string> ModeratorIds { get; set; } = new();
    
    public List<string> MemeChannelIds { get; set; } = new();
    
    public List<string> DeafenedChannelIds { get; set; } = new();
    
    public string? XkcdChannelId { get; set; }
    
    public int Volume { get; set; } = 50;
    
    public DiscordServer(string server)
    {
        Server = server;
    }
}