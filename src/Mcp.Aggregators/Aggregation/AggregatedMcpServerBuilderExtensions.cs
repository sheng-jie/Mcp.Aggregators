using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Mcp.Aggregators.Aggregation;

public static class AggregatedMcpServerBuilderExtensions
{

    public static IMcpServerBuilder WithAggregatedHttpServerTransport(this IMcpServerBuilder builder)
    {
        Throw.IfNull(builder);

        builder.Services.TryAddSingleton<McpClientsFactory>();
        builder.Services.TryAddSingleton<McpToolsHandler>();

        builder.WithHttpTransport();

        builder.ConfigureHandlers();

        return builder;
    }

    public static IMcpServerBuilder WithAggregatedStdioServerTransport(this IMcpServerBuilder builder)
    {
        Throw.IfNull(builder);

        builder.Services.AddSingleton<McpClientsFactory>();
        builder.Services.AddSingleton<McpToolsHandler>();

        builder.WithStdioServerTransport();
        builder.ConfigureHandlers();

        return builder;
    }
    

    private static void ConfigureHandlers(this IMcpServerBuilder builder)
    {
        builder.WithListToolsHandler(
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
    }
}

