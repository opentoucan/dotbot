using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xkcd.Job.Extensions;
using Xkcd.Job.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging();

builder.AddApplicationServices();
builder.AddDatabase();
builder.AddMassTransit();
builder.AddHttpClient();

using IHost host = builder.Build();

using var scope = host.Services.CreateScope();
var xkcdNotificationService = scope.ServiceProvider.GetRequiredService<IXkcdNotificationService>();

await xkcdNotificationService.CheckAndNotify();
