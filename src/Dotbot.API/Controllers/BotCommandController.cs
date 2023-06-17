using Dotbot.Services;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Dotbot.Controllers;

[Microsoft.AspNetCore.Components.Route("api/v1/[controller]")]
[ApiController]
public class BotCommandController : ControllerBase
{
    private readonly IBotCommandService _botCommandService;

    public BotCommandController(IBotCommandService botCommandService)
    {
        _botCommandService = botCommandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBotCommand(string serviceId, string name)
    {
        var result = await _botCommandService.FindBotCommand(serviceId, name);
        return result.ToActionResult();
    }
}