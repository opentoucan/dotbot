using ServiceDefaults;
using Xkcd.Job;
using Xkcd.Job.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();

builder.AddApplicationServices();
builder.AddDatabase();
builder.AddMassTransit();
builder.AddHttpClient();
builder.Services.AddHostedService<Worker>();
using IHost host = builder.Build();
await host.RunAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
