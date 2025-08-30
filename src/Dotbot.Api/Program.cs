using Dotbot.Api.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationServices();
var app = builder.Build();

app.UseDefaultOpenApi();
app.MapDefaultEndpoints();
app.ConfigureDiscordWebApplication();

await app.RunAsync();