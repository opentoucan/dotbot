using AutoMapper;
using Dotbot.Gateway.Application.InteractionCommands.SlashCommands;
using Dotbot.Gateway.Dto.Requests.Discord;

namespace Dotbot.Gateway.Dto.AutoMapper;

public class XkcdCommandProfile : Profile
{
    public XkcdCommandProfile()
    {
        CreateMap<InteractionRequest, XkcdCommand>()
            .ForMember(dest => dest.ComicNumber, opt =>
            {
                opt.PreCondition(src => src?.Data?.Options?.FirstOrDefault()?.Value is not null);
                opt.MapFrom(src => (int)src.Data!.Options!.FirstOrDefault()!.Value!);
            });
    }
}