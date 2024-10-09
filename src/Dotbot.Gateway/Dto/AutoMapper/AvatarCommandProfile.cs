using AutoMapper;
using Dotbot.Gateway.Application.InteractionCommands.SlashCommands;
using Dotbot.Gateway.Dto.Requests.Discord;

namespace Dotbot.Gateway.Dto.AutoMapper;

public class AvatarCommandProfile : Profile
{
    public AvatarCommandProfile()
    {
        CreateMap<InteractionRequest, AvatarCommand>()
            .ForMember(dest => dest.AvatarId, opt =>
            {
                opt.PreCondition(src => src.Data?.Resolved?.Users?.FirstOrDefault().Value is not null);
                opt.MapFrom(src => src.Data!.Resolved!.Users!.FirstOrDefault().Value.Avatar);
            })
            .ForMember(dest => dest.TargetUserId, opt =>
            {
                opt.PreCondition(src => src.Data?.Resolved?.Users?.FirstOrDefault().Value is not null);
                opt.MapFrom(src => src.Data!.Resolved!.Users!.FirstOrDefault().Value.Id);
            })
            .ForMember(dest => dest.TargetUsername, opt =>
            {
                opt.PreCondition(src => src.Data?.Resolved?.Users?.FirstOrDefault().Value is not null);
                opt.MapFrom(src => src.Data!.Resolved!.Users!.FirstOrDefault().Value.Username);
            });
    }
}