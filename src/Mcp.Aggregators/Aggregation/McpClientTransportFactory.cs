using Mcp.Aggregators.Configuration;
using ModelContextProtocol.Client;

namespace Mcp.Aggregators.Aggregation;

public static class McpClientTransportFactory
    {
        public static IClientTransport Create(string name, McpServerConfig config)
        {
            if (config.Type == "stdio")
            {
                return new StdioClientTransport(new StdioClientTransportOptions
                {
                    Name = $"{name}-client",
                    Command = config.Command,
                    Arguments = config.Args,
                    EnvironmentVariables = config.Env
                });
            }
            else if (config.Type == "sse" || config.Type == "http")
            {
                return new SseClientTransport(new SseClientTransportOptions
                {
                    Name = $"{name}-client",
                    Endpoint = new Uri(config.Url)
                });
            }
            else
            {
                throw new InvalidOperationException($"Unsupported MCP server type: {config.Type}");
            }
        }
    }
