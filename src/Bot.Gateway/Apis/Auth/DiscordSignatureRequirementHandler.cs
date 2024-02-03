using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Bot.Gateway.Apis.Auth;

public class DiscordSignatureRequirementHandler : AuthorizationHandler<DiscordSignatureRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DiscordSignatureRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            var signature = httpContext.Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
            var timestamp = httpContext.Request.Headers["X-Signature-Timestamp"].FirstOrDefault();
            var key = Environment.GetEnvironmentVariable("Discord__PublicKey");
            var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        
            if (signature is null 
                || timestamp is null 
                || key is null 
                || !new DiscordRestClient().IsValidHttpInteraction(key, signature, timestamp, json))
            {
                context.Fail();
                return;
            }
            context.Succeed(requirement);
            return;
        }
        context.Fail();
    }
}