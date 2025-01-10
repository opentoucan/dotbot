using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.Services;
using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Services;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();
var app = builder.Build();

app.AddModules(typeof(Program).Assembly);

app.UseDefaultOpenApi();

app.UseHttpInteractions("/interactions");

app.MapDefaultEndpoints();

using var scope = app.Services.CreateScope();

var registrationService = scope.ServiceProvider.GetRequiredService<IRegistrationService>();

await registrationService.Register();
await app.RunAsync();