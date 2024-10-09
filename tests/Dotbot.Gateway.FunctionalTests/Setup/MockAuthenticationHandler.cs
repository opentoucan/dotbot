using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.FunctionalTests.Setup;

#nullable disable
public class MockSchemeProvider : AuthenticationSchemeProvider
{
    public MockSchemeProvider(IOptions<AuthenticationOptions> options)
        : base(options)
    {
    }

    protected MockSchemeProvider(
        IOptions<AuthenticationOptions> options,
        IDictionary<string, AuthenticationScheme> schemes
    )
        : base(options, schemes)
    {
    }

    public override Task<AuthenticationScheme> GetSchemeAsync(string name)
    {
        if (name == "Test")
        {
            var scheme = new AuthenticationScheme(
                "Test",
                "Test",
                typeof(MockAuthenticationHandler)
            );
            return Task.FromResult(scheme);
        }

        return base.GetSchemeAsync(name)!;
    }
}
public class MockAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "service") }; 
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}