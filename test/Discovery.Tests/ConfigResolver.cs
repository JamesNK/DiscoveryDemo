using Microsoft.Extensions.Configuration;

namespace Discovery.Tests;

internal sealed class ConfigResolver : Resolver
{
    private readonly Uri _address;
    private readonly IConfiguration _configuration;

    public ConfigResolver(Uri address, IConfiguration configuration)
    {
        _address = address;
        _configuration = configuration;
    }

    public override void Start(Action<ResolverResult> listener)
    {
        try
        {
            PublishConfigResult(_configuration, _address.AbsolutePath, listener);

            _configuration.GetReloadToken().RegisterChangeCallback(callback: state =>
            {
                PublishConfigResult(_configuration, _address.AbsolutePath, listener);
            }, state: null);
        }
        catch (Exception ex)
        {
            listener(ResolverResult.ForFailure(new Status(StatusCode.Internal, "Resolve failed", ex)));
        }

        static void PublishConfigResult(IConfiguration configuration, string path, Action<ResolverResult> listener)
        {
            path = path.TrimStart('/');
            var section = configuration.GetRequiredSection(path);

            listener(ResolverResult.ForResult(new List<BalancerAddress>
            {
                new BalancerAddress(host: section["host"]!, port: Convert.ToInt32(section["port"]))
            }));
        }
    }
}