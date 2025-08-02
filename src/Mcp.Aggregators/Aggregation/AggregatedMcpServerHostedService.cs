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

            // 优化：预先过滤启用的服务器，减少不必要的并发任务
            var enabledServers = mcpServerOptions.Value.McpServers
                .Where(kv => kv.Value.Enabled.GetValueOrDefault(true))
                .ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = stoppingToken
            };

            await Parallel.ForEachAsync(
                enabledServers,
                parallelOptions,
                async (serverConfig, ct) =>
                {
                    var serverId = serverConfig.Key;
                    var config = serverConfig.Value;

                    var clientWrapper = new McpClientWrapper(serverId, config, loggerFactory);
                    await clientWrapper.InitializeAsync().ConfigureAwait(false);
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