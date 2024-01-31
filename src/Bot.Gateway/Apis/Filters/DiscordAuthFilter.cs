using Discord.Rest;

namespace Bot.Gateway.Apis.Filters;

public class DiscordAuthFilter : IEndpointFilter
{
    private readonly ILogger _logger;

    public DiscordAuthFilter(ILogger<DiscordAuthFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        _logger.LogInformation("Authenticating request: {request}", context.HttpContext.Request.Path);
        
        context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        var signature = context.HttpContext.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
        var timestamp = context.HttpContext.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();
        var key = Environment.GetEnvironmentVariable("Discord__PublicKey");
        var json = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
        
        if (signature is null 
            || timestamp is null 
            || key is null 
            || !new DiscordRestClient().IsValidHttpInteraction(key, signature, timestamp, json))
        {
            _logger.LogWarning("Failed to authenticate request: {request}", context.HttpContext.Request.Path);
            return Results.Unauthorized();
        }
        
        _logger.LogInformation("Request authenticated: {request}", context.HttpContext.Request.Path);
        return await next(context);

    }
}