using AutoMapper;
using Bot.Gateway.Application.InteractionCommands.SlashCommands;
using Bot.Gateway.Dto.Requests.Discord;

namespace Bot.Gateway.Dto.AutoMapper;

public class PingCommandProfile : Profile
{
    public PingCommandProfile()
    {
        CreateMap<InteractionRequest, PingCommand>();
    }
}