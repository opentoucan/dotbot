using AutoMapper;
using Dotbot.Gateway.Application.InteractionCommands.SlashCommands;
using Dotbot.Gateway.Dto.Requests.Discord;

namespace Dotbot.Gateway.Dto.AutoMapper;

public class RetrieveCustomCommandsProfile : Profile
{
    public RetrieveCustomCommandsProfile()
    {
        CreateMap<InteractionRequest, RetrieveCustomCommand>()
            .ForMember(dest => dest.CustomCommandName, opt =>
            {
                opt.PreCondition(src => src?.Data?.Options?.FirstOrDefault()?.Value is not null);
                opt.MapFrom(src => src.Data!.Options!.FirstOrDefault()!.Value!.ToString());
            })
            .ForMember(dest => dest.GuildId, opt =>
            {
                opt.AllowNull();
                opt.PreCondition(src => src.Guild?.Id is not null);
                opt.MapFrom(src => src.Guild!.Id);
            })
            .ForMember(dest => dest.DirectMessageChannelId, opt =>
            {
                opt.AllowNull();
                opt.PreCondition(src => src.User?.Id is not null);
                opt.MapFrom(src => src.User!.Id);
            });
    }
}