using Dotbot.Api.Discord.Extensions;
using Dotbot.Api.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.ConfigureDiscordServices();
var app = builder.Build();

app.UseDefaultOpenApi();
app.MapDefaultEndpoints();
app.ConfigureDiscordWebApplication();

await app.RunAsync();