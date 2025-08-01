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

// read the MCP server configuration from the args
builder.Configuration.AddCommandLine(args);

var mcpFilePath = args.FirstOrDefault(a => a.StartsWith("--mcp-file="))?.Split('=')[1];
if (!string.IsNullOrEmpty(mcpFilePath) && File.Exists(mcpFilePath))
{
    builder.Configuration.AddJsonFile(mcpFilePath, optional: false, reloadOnChange: true);
}
else
{
    // Fallback to the default mcp.json in the current directory
    builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("mcp.json", optional: false, reloadOnChange: true);
}


// Register the configuration for MCP servers - bind to root since McpServerOptions expects the full JSON
builder.Services.Configure<McpServerConfigOptions>(builder.Configuration);
builder.Services.AddSingleton<IValidateOptions<McpServerConfigOptions>, McpServerConfigOptionsValidator>();


// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithAggregatedStdioServerTransport();

var app = builder.Build();

await app.RunAsync();
