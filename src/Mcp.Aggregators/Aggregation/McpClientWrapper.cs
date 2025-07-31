using System;
using Mcp.Aggregators.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Mcp.Aggregators.Aggregation;

public class McpClientWrapper
{
    public string Name { get; private set; }

    private readonly McpServerConfig _config;

    private readonly IClientTransport _clientTransport;
    public IMcpClient McpClient { get; private set; }
    private readonly ILogger<McpClientWrapper>? _logger;
    private readonly Dictionary<string, TaskCompletionSource<string>> _initializationRequests;

    public McpClientWrapper(string name, McpServerConfig config, ILoggerFactory? loggerFactory = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = loggerFactory?.CreateLogger<McpClientWrapper>();
        _initializationRequests = new Dictionary<string, TaskCompletionSource<string>>();

        // 创建对应的传输客户端

        _clientTransport = McpClientTransportFactory.Create(name, config);

        // _transportClient = McpTransportClientFactory.Create(config, loggerFactory);
        // _transportClient.MessageReceived += OnMessageReceived;



        // var transport = await _clientTransport.ConnectAsync();

        // _mcpClient = await McpClientFactory.CreateAsync(_clientTransport);
    }

    public async Task InitializeAsync()
    {
        if (McpClient != null)
        {
            throw new InvalidOperationException("Client is already initialized.");
        }

        // Connect to the MCP server and create the client
        McpClient =  await McpClientFactory.CreateAsync(_clientTransport).ConfigureAwait(false);

        // Optionally, you can perform additional initialization here
        _logger?.LogInformation($"MCP Client '{Name}' initialized successfully.");
    }

}
