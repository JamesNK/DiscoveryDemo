using Microsoft.Extensions.Configuration;

namespace Discovery.Tests;

internal sealed class ConfigResolverFactory : ResolverFactory
{
    private readonly IConfiguration _configuration;

    public override string Name => "config";

    public ConfigResolverFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override Resolver Create(ResolverOptions options)
    {
        return new ConfigResolver(options.Address, _configuration);
    }
}