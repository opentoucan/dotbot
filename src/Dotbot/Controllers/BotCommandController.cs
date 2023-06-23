using Ardalis.GuardClauses;
using Dotbot.Dtos;
using Dotbot.Infrastructure.Repositories;
using Dotbot.Models;
using Dotbot.SeedWork;
using Dotbot.Services;
using Dotbot.ViewModels;
using FluentResults.Extensions.AspNetCore;
using FuzzySharp;
using Microsoft.AspNetCore.Mvc;

namespace Dotbot.Controllers;

[Microsoft.AspNetCore.Components.Route("api/v1/[controller]")]
[ApiController]
public class BotCommandController : ControllerBase
{
    private readonly IBotCommandRepository _botCommandRepository;

    public BotCommandController(IBotCommandRepository botCommandRepository)
    {
        _botCommandRepository = botCommandRepository;
    }

    [Route("{serviceId}")]
    [HttpGet]
    public async Task<IActionResult> GetBotCommand(string serviceId, [FromQuery] string name)
    {
        var result = _botCommandRepository.GetCommand(serviceId, name);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [Route("save")]
    [HttpPut]
    public async Task<IActionResult> SaveBotCommand([FromBody] SaveBotCommand saveBotCommand)
    {
        Guard.Against.NullOrWhiteSpace(saveBotCommand.ServiceId);
        Guard.Against.NullOrWhiteSpace(saveBotCommand.Name);
        Guard.Against.NullOrWhiteSpace(saveBotCommand.Content);
        Guard.Against.NullOrWhiteSpace(saveBotCommand.CreatorId);

        var botCommand = new BotCommand(
            saveBotCommand.ServiceId, 
            saveBotCommand.Name,
            saveBotCommand.Content,
            Enumeration.FromValue<BotCommandType>(saveBotCommand.CommandType), 
            saveBotCommand.CreatorId);
        try
        {
            await _botCommandRepository.SaveCommand(botCommand);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("commands/{serviceId}")]
    [HttpGet]
    public async Task<IActionResult> GetAllCommands(string serviceId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0 )
    {
        var botCommands = await _botCommandRepository.GetCommands(serviceId, pageIndex, pageSize);
        if (botCommands.IsFailed)
            return BadRequest();

        var count = await _botCommandRepository.GetCommandCount(serviceId);
        var model = new PaginatedItemsViewModel<BotCommand>(pageIndex, pageSize, count, botCommands.Value);
        return Ok(model);
    }

    [Route("search/{serviceId}")]
    [HttpGet]
    public async Task<IActionResult> FindSimilarCommands(string serviceId, [FromQuery] string searchTerm, [FromQuery] int limit = 20, [FromQuery] int cutoff = 50)
    {
        var allNames = await _botCommandRepository.GetAllNames(serviceId);
        
        var matches = Process.ExtractTop(searchTerm, allNames, cutoff: cutoff, limit: limit);

        return Ok(matches.Select(x => new FuzzySearchViewModel(x.Value, x.Score)).ToList());
    }
}