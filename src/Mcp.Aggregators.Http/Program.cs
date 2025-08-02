using Mcp.Aggregators.Aggregation;
using Mcp.Aggregators.Configuration;
using Mcp.Aggregators.Http.Components;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


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


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMcpServer()
    .WithAggregatedHttpServerTransport();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForErrors: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapMcp("/mcp");

app.Run();
