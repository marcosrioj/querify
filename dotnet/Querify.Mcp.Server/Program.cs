using Querify.Mcp.Server.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddQuerifyMcpServer(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
