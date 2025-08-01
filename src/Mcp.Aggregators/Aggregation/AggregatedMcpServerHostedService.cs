using System.Collections.Concurrent;
using Mcp.Aggregators.Aggregation;
using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


internal sealed class AggregatedMcpServerHostedService(
    IOptions<McpServerConfigOptions> mcpServerOptions,
    ILoggerFactory loggerFactory,
    IHostApplicationLifetime? lifetime = null) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = loggerFactory.CreateLogger<AggregatedMcpServerHostedService>();

        try
        {
            logger?.LogInformation("Starting Aggregated MCP Server...");
            var clientWrappers = new ConcurrentBag<McpClientWrapper>();

            // 并行初始化每个 MCP 客户端
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = stoppingToken
            };

            await Parallel.ForEachAsync(
                mcpServerOptions.Value.McpServers,
                parallelOptions,
                async (serverConfig, ct) =>
                {
                    var serverId = serverConfig.Key;
                    // Skip disabled servers
                    if (!serverConfig.Value.Enabled.GetValueOrDefault(true))
                    {
                        logger?.LogInformation($"Skipping disabled MCP server: {serverId}");
                        return;
                    }
                    var config = serverConfig.Value;

                    // Register each MCP client wrapper with its configuration
                    var clientWrapper = new McpClientWrapper(serverId, config, loggerFactory);
                    await clientWrapper.InitializeAsync();
                    clientWrappers.Add(clientWrapper);
                }
            ).ConfigureAwait(false);

            var factory = new AggregatedMcpServerFactory(clientWrappers);
            var server = factory.CreateAggregatedServer();

            logger?.LogInformation("Aggregated MCP Server started successfully.");
            await server.RunAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to start Aggregated MCP Server.");
            throw; // Re-throw the exception to ensure the host stops
        }
        finally
        {
            lifetime?.StopApplication();
        }
    }
}