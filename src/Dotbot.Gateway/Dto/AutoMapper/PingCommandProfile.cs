using AutoMapper;
using Dotbot.Gateway.Application.InteractionCommands.SlashCommands;
using Dotbot.Gateway.Dto.Requests.Discord;

namespace Dotbot.Gateway.Dto.AutoMapper;

public class PingCommandProfile : Profile
{
    public PingCommandProfile()
    {
        CreateMap<InteractionRequest, PingCommand>();
    }
}