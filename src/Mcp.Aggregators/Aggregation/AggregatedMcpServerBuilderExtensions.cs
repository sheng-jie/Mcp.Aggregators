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

        builder.Services.AddHostedService<AggregatedMcpServerHostedService>();

        return builder;
    }
}

