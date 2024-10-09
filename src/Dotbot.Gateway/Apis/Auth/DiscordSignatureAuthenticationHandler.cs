using System.Security.Claims;
using System.Text.Encodings.Web;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Apis.Auth;

public class DiscordSignatureAuthenticationHandler(
    ILogger<DiscordSignatureAuthenticationHandler> logger,
    IOptionsMonitor<DiscordSignatureAuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder)
    : AuthenticationHandler<DiscordSignatureAuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {            
        logger.LogDebug("Attempting to authenticate http request {path}", Request.Path);
        Request.EnableBuffering();
        var signature = Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
        var timestamp = Request.Headers["X-Signature-Timestamp"].FirstOrDefault();
        var key = Options.PublicKey;
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        
        if (signature is null 
            || timestamp is null 
            || key is null 
            || !new DiscordRestClient().IsValidHttpInteraction(key, signature, timestamp, json))
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
        Request.Body.Seek(0, SeekOrigin.Begin);
        var claims = new[] { new Claim(ClaimTypes.Name, "service") }; 
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}

public class DiscordSignatureAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string? PublicKey { get; set; }
}
