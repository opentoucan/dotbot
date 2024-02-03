using System.Security.Claims;
using System.Text.Encodings.Web;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Bot.Gateway.Apis.Auth;

public class DiscordSignatureAuthenticationHandler : AuthenticationHandler<DiscordSignatureAuthenticationSchemeOptions>
{
    public DiscordSignatureAuthenticationHandler(IOptionsMonitor<DiscordSignatureAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {            
        var signature = Request.Headers["X-Signature-Ed25519"].FirstOrDefault();
        var timestamp = Request.Headers["X-Signature-Timestamp"].FirstOrDefault();
        var key = Environment.GetEnvironmentVariable("Discord__PublicKey");
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        
        if (signature is null 
            || timestamp is null 
            || key is null 
            || !new DiscordRestClient().IsValidHttpInteraction(key, signature, timestamp, json))
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
        
        var claims = new[] { new Claim(ClaimTypes.Name, "service") }; 
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}

public class DiscordSignatureAuthenticationSchemeOptions : AuthenticationSchemeOptions;
