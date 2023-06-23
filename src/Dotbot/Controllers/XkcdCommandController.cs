using Ardalis.GuardClauses;
using Dotbot.Models;
using Dotbot.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotbot.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class XkcdCommandController : ControllerBase
{
    private readonly IXkcdCommandService _xkcdCommandService;

    public XkcdCommandController(IXkcdCommandService xkcdCommandService)
    {
        _xkcdCommandService = xkcdCommandService;
    }

    [Route("{comicNumber}")]
    [HttpGet]
    public ActionResult<XkcdComic> Get(int comicNumber)
    {
        try
        {
            Guard.Against.Negative(comicNumber);
        }
        catch (ArgumentException ex)
        {
            return BadRequest("Comic number cannot be a negative value");
        }

        var comic = _xkcdCommandService.GetXkcd(comicNumber);
        if (comic == null)
            return NotFound($"No comic found for comic number: {comicNumber}");
        return Ok(comic);
    }

    [Route("latest")]
    [HttpGet]
    public ActionResult<XkcdComic> GetLatest()
    {
        var xkcdComic = _xkcdCommandService.GetXkcd();
        return Ok(xkcdComic);
    }
}