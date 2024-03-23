using Bot.Gateway.Apis;
using Bot.Gateway.Extensions;
using Bot.Gateway.Services;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();
builder.Services.AddHostedService<DiscordRegistrationService>();

var app = builder.Build();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();
app.MapGroup("/api/interactions")
    .MapDiscordInteractionApi()
    .RequireAuthorization("DiscordSignature");
app.Run();