using Dotbot.Gateway.Apis;
using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.Services;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();
var healthCheckMode = builder.Configuration.GetValue<bool>("TestMode");
if(!healthCheckMode)
    builder.Services.AddHostedService<DiscordRegistrationService>();

var app = builder.Build();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();
app.MapGroup("/api/interactions")
    .MapDiscordInteractionApi()
    .RequireAuthorization("DiscordSignature");
app.Run();