using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Mcp.Aggregators.Aggregation;

public static class AggregatedMcpServerBuilderExtensions
{
    public static IMcpServerBuilder WithAggregatedStdioServerTransport(this IMcpServerBuilder builder)
    {
        Throw.IfNull(builder);

        builder.Services.AddSingleton<McpClientsFactory>();
        builder.Services.AddSingleton<McpToolsHandler>();

        builder.WithStdioServerTransport()
        .WithListToolsHandler(
            async (context, stoppingToken) =>
            {
                if (context.Services is null)
                    throw new InvalidOperationException("Service provider is null in the current context.");
                McpToolsHandler mcpToolsHandler = context.Services.GetRequiredService<McpToolsHandler>();
                return await mcpToolsHandler.ListAggregatedToolsAsync(context, stoppingToken);
            });

        builder.WithCallToolHandler(async (context, cancellationToken) =>
        {
            if (context.Services is null)
                throw new InvalidOperationException("Service provider is null in the current context.");
            var mcpToolsHandler = context.Services.GetRequiredService<McpToolsHandler>();
            return await mcpToolsHandler.CallAggregatedToolAsync(context, cancellationToken);
        });

        return builder;
    }
}

