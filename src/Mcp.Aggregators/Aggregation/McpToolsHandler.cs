using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Mcp.Aggregators.Aggregation;

public sealed class McpToolsHandler
{
    private readonly McpClientsFactory _mcpClientsFactory;
    private readonly ILogger<McpToolsHandler> _logger;

    public McpToolsHandler(McpClientsFactory mcpClientsFactory, ILogger<McpToolsHandler> logger)
    {
        _mcpClientsFactory = mcpClientsFactory;
        _logger = logger;
    }
    


    public async ValueTask<ListToolsResult> ListAggregatedToolsAsync(RequestContext<ListToolsRequestParams> request, CancellationToken cancellation)
    {
        var clientWrappers = await _mcpClientsFactory.GetOrCreateClientsAsync(cancellation).ConfigureAwait(false);
        var toolLists = await Task.WhenAll(
            clientWrappers.Select(async clientWrapper =>
            {
                var clientTools = await clientWrapper.McpClient.ListToolsAsync(cancellationToken: cancellation);
                return clientTools.Select(tool =>
                {
                    var protocolTool = tool.ProtocolTool;
                    protocolTool.Name = $"{clientWrapper.Name}.{protocolTool.Name}";
                    return protocolTool;
                });
            })
        ).ConfigureAwait(false);

        return new ListToolsResult
        {
            Tools = toolLists.SelectMany(t => t).ToList()
        };
    }

    public async ValueTask<CallToolResult> CallAggregatedToolAsync(RequestContext<CallToolRequestParams> request, CancellationToken cancellation)
    {
        var clientWrappers = await _mcpClientsFactory.GetOrCreateClientsAsync(cancellation).ConfigureAwait(false);

        var splitToolNames = request.Params?.Name.Split('.');
        if (splitToolNames == null || splitToolNames.Length != 2)
        {
            throw new ArgumentException("Tool name must be in the format 'ClientName.ToolName'.", nameof(request));
        }

        var targetClientName = splitToolNames[0];
        var toolName = splitToolNames[1];
        var clientWrapper = clientWrappers.FirstOrDefault(cw => cw.Name == targetClientName);

        if (clientWrapper == null)
        {
            throw new InvalidOperationException($"No client found for tool '{toolName}'");
        }

        var convertedArguments = request.Params?.Arguments?
            .ToDictionary(
            kvp => kvp.Key,
            kvp => (object?)kvp.Value);

        return await clientWrapper.McpClient.CallToolAsync(
            toolName: toolName,
            arguments: convertedArguments,
            cancellationToken: cancellation);
    }
}
