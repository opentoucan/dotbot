using AutoMapper;
using Bot.Gateway.Application.InteractionCommands.SlashCommands;
using Bot.Gateway.Dto.Requests.Discord;

namespace Bot.Gateway.Dto.AutoMapper;

public class SaveCustomCommandProfile : Profile
{
    public SaveCustomCommandProfile()
    {
        CreateMap<InteractionRequest, SaveCustomCommand>()
            .ForMember(dest => dest.GuildId, opt =>
            {
                opt.PreCondition(src => src.Guild?.Id is not null);
                opt.AllowNull();
                opt.MapFrom(src => src.Guild!.Id);
            })
            .ForMember(dest => dest.DirectMessageChannelId, opt =>
            {
                opt.PreCondition(src => src.Channel?.Id is not null);
                opt.AllowNull();
                opt.MapFrom(src => src.Channel!.Id);
            })
            .ForMember(dest => dest.SenderId, opt => 
                opt.MapFrom(src => src.Member != null ? src.Member.User.Id : src.User!.Id!))
            .ForMember(dest => dest.CustomCommandName, opt =>
            {
                opt.PreCondition(src => src.Data?.Options?.FirstOrDefault()?.SubOptions?.FirstOrDefault(o => o.Name == "name")?.Value is not null);
                opt.MapFrom(src => src.Data!.Options!.FirstOrDefault()!.SubOptions!.FirstOrDefault(o => o.Name == "name")!.Value);
            })
            .ForMember(dest => dest.TextContent, opt =>
            {
                opt.AllowNull();
                opt.PreCondition(src => src.Data?.Options?.FirstOrDefault()?.SubOptions?.FirstOrDefault(o => o.Name == "text")?.Value is not null);
                opt.MapFrom(src => src.Data!.Options!.FirstOrDefault()!.SubOptions!.FirstOrDefault(o => o.Name == "text")!.Value);
            })
            .ForMember(dest => dest.FileNameUrlDictionary, opt =>
            {
                opt.PreCondition(src => src.Data?.Resolved?.Attachments is not null);
                opt.MapFrom(src => src.Data!.Resolved!.Attachments!
                    .ToDictionary(a => 
                        $"{Guid.NewGuid()}{Path.GetExtension(a.Value.Url).Split("?", StringSplitOptions.TrimEntries)[0]}", 
                        a => a.Value.Url));
            });
    }
}