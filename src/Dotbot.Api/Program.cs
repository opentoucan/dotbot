using Dotbot.Api.Application.Api;
using Dotbot.Api.Extensions;
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