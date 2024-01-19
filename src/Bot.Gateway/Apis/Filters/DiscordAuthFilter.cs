using Discord.Rest;

namespace Bot.Gateway.Apis.Filters;

public class DiscordAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        var signature = context.HttpContext.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
        var timestamp = context.HttpContext.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();
        var key = Environment.GetEnvironmentVariable("Discord__PublicKey");
        var json = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
        Console.WriteLine(json);
        var restClient = new DiscordRestClient();
        var verified = restClient.IsValidHttpInteraction(key, signature, timestamp, json);
        
        return !verified
            ? Results.Unauthorized()
            : await next(context);

    }
}