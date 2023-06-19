using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discovery.Tests;

public class DiscoveryHttpHandlerTests
{
    [Fact]
    public async Task DiscoveryHttpHandler_ConfigSource_AddressResolved()
    {
        // Arrange
        var services = ConfigureServices();

        var testHandler = new TestHandler();

        services
            .AddHttpClient("TestClient")
            .AddHttpMessageHandler<DiscoveryHttpHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => testHandler);

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = clientFactory.CreateClient("TestClient");

        // Act

        // config = resolve address from IConfiguration
        // addresses:backend = path to values in IConfiguration
        var sdf = await httpClient.GetAsync("config:///addresses:backend/api/product/1");

        // Assert
        Assert.Equal("https://localhost:80/api/product/1", testHandler.RequestMessage!.RequestUri!.ToString());
    }

    private static ServiceCollection ConfigureServices()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddOptions();
        services.AddSingleton<ResolverRegistry>();
        services.AddTransient<DiscoveryHttpHandler>();
        services.AddSingleton<ResolverFactory, ConfigResolverFactory>();
        return services;
    }
}