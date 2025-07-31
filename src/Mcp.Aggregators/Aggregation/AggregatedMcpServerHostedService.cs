using Mcp.Aggregators.Aggregation;
using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;


internal sealed class AggregatedMcpServerHostedService(IOptions<McpServerConfigOptions> mcpServerOptions, IHostApplicationLifetime? lifetime = null) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var clientWrappers = new List<McpClientWrapper>();
            foreach (var serverConfig in mcpServerOptions.Value.McpServers)
            {
                var serverId = serverConfig.Key;
                var config = serverConfig.Value;

                // Register each MCP client wrapper with its configuration

                var clientWrapper = new McpClientWrapper(serverId, config);

                await clientWrapper.InitializeAsync();

                clientWrappers.Add(clientWrapper);
            }

            var factory = new AggregatedMcpServerFactory(clientWrappers);

            var server = factory.CreateAggregatedServer();

            await server.RunAsync(stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            lifetime?.StopApplication();
        }
    }
}