using System.Runtime.Serialization;

namespace Discord.Application.Models;

public class SaveBotCommand
{
    [DataMember]
    public string ServiceId { get; set; }
    
    [DataMember]
    public string CreatorId { get; set; }
    
    [DataMember]
    public string Name { get; set; }
    
    [DataMember]
    public string Content { get; set; }
    
    [DataMember]
    public int CommandType { get; set; }
}