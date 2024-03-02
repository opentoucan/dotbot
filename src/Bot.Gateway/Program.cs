using Bot.Gateway.Apis;
using Bot.Gateway.Extensions;
using Discord;
using Discord.Rest;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();

var app = builder.Build();

var discordClient = app.Services.GetRequiredService<DiscordRestClient>();
await discordClient.LoginAsync(TokenType.Bot, builder.Configuration.GetValue<string>("DiscordSettings:BotToken"));
await discordClient.RegisterCommands(builder);
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

app.UseDefaultOpenApi();
app.MapDefaultEndpoints();
app.MapGraphQL();
app.MapGroup("/api/interactions")
    .MapDiscordInteractionApi()
    .RequireAuthorization("DiscordSignature");
app.Run();

public partial class Program { }