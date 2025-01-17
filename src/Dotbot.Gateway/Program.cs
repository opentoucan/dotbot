using Dotbot.Gateway.Application.Api;
using Dotbot.Gateway.Extensions;
using NetCord.Hosting.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();
var app = builder.Build();

app.UseDefaultOpenApi();
app.UseHttpInteractions("/interactions");
app.MapDefaultEndpoints();
app.MapSlashCommandApi();

await app.RunAsync();