using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Mcp.Aggregators.Aggregation;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("mcp.json", optional: false, reloadOnChange: true);

// Register the configuration for MCP servers - bind to root since McpServerOptions expects the full JSON
builder.Services.Configure<McpServerConfigOptions>(builder.Configuration);
builder.Services.AddSingleton<IValidateOptions<McpServerConfigOptions>, McpServerConfigOptionsValidator>();

// builder.Services.AddOptions<McpServerOptions>()
//     .Bind(builder.Configuration.GetSection("mcpServers"))
//     .ValidateDataAnnotations()
//     .ValidateOnStart();


// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithAggregatedStdioServerTransport();
    // .WithStdioServerTransport()
    // .WithTools<RandomNumberTools>();

var app = builder.Build();

// var mcpServerOptions = app.Services.GetRequiredService<IOptions<McpServerConfigOptions>>().Value;

// foreach (var serverConfig in mcpServerOptions.McpServers)
// {
//     var serverId = serverConfig.Key;
//     var config = serverConfig.Value;

//     // Register each MCP client wrapper with its configuration

//     var clientWrapper = new McpClientWrapper(serverId, config, app.Services.GetService<ILoggerFactory>());

//     await clientWrapper.InitializeAsync();
// }


// Run the application.

await app.RunAsync();
