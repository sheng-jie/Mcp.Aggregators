using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mcp.Aggregators.Aggregation;

public sealed class McpClientsFactory
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly List<McpClientWrapper> _clientWrappers = new();

    private readonly McpServerConfigOptions _mcpServerConfigOptions;

    public McpClientsFactory(IOptions<McpServerConfigOptions> options, ILoggerFactory loggerFactory)
    {
        _mcpServerConfigOptions = options.Value;

        _loggerFactory = loggerFactory;
    }
    public async Task<List<McpClientWrapper>> GetOrCreateClientsAsync(CancellationToken cancellationToken = default)
    {
        if (_mcpServerConfigOptions.McpServers is null || !_mcpServerConfigOptions.McpServers.Any())
        {
            throw new InvalidOperationException("No MCP servers configured.");
        }
        
        if (_clientWrappers.Any())
        {
            // 如果已经初始化过客户端，直接返回现有的客户端列表
            return _clientWrappers;
        }

        var clients = await McpClientInitializer.InitializeClientsAsync(
            _mcpServerConfigOptions.McpServers, _loggerFactory, cancellationToken)
            .ConfigureAwait(false);

        _clientWrappers.AddRange(clients);
        return _clientWrappers;
    }

}
