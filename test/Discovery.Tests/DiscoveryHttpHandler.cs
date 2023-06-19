using Discovery.Internal;
using Microsoft.Extensions.Logging;

namespace Discovery.Tests;

internal sealed class DiscoveryHttpHandler : DelegatingHandler
{
    private readonly ResolverRegistry _resolverRegistry;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ResolverState> _resolverCache;

    public DiscoveryHttpHandler(ResolverRegistry resolverRegistry, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _resolverRegistry = resolverRegistry;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
        _resolverCache = new Dictionary<string, ResolverState>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scheme = request.RequestUri!.Scheme;

        var configPath = request.RequestUri.AbsolutePath.TrimStart('/');
        var requestPath = string.Empty;
        var end = configPath.IndexOf('/');
        if (end != -1)
        {
            requestPath = configPath.Substring(end);
            configPath = configPath.Substring(0, end);
        }

        // This handler is designed to be generic and universial to whatever discovery address is passed to it.
        // An alternative approach is to limit the handler to one resolver (aka domain) when it is created
        // and support resolving it, and throw an error for any other domain.
        if (!_resolverCache.TryGetValue(scheme, out var resolverState))
        {
            var resolverOptions = new ResolverOptions(
                address: new Uri($"{scheme}:///{configPath}"),
                defaultPort: 80,
                loggerFactory: _loggerFactory,
                serviceProvider: _serviceProvider);
            var resolver = _resolverRegistry.CreateResolver(scheme, resolverOptions);
            if (resolver is null)
            {
                throw new InvalidOperationException($"Couldn't create a resolver for the scheme '{request.RequestUri.Scheme}'.");
            }

            _resolverCache[scheme] = resolverState = new ResolverState(resolver);
        }

        var address = await resolverState.AddressTask;

        var uriBuilder = new UriBuilder(request.RequestUri);
        uriBuilder.Scheme = "https"; // TODO: Add support for configuring whether HTTPS is required.
        uriBuilder.Host = address.EndPoint.Host;
        uriBuilder.Port = address.EndPoint.Port;
        uriBuilder.Path = requestPath;
        uriBuilder.Query = request.RequestUri.Query;

        if (address.Attributes.TryGetValue(ProtocolHelpers.HostOverrideKey, out var hostOverride))
        {
            // Allow a resolver to specify a host name header that an IP address maps to.
            // The DNS resolver will set this.
            request.Headers.TryAddWithoutValidation("Host", hostOverride);
        }

        request.RequestUri = uriBuilder.Uri;

        return await base.SendAsync(request, cancellationToken);
    }

    private sealed class ResolverState
    {
        private readonly Resolver _resolver;
        private readonly TaskCompletionSource<BalancerAddress> _addressTcs;

        public ResolverState(Resolver resolver)
        {
            _resolver = resolver;
            _addressTcs = new TaskCompletionSource<BalancerAddress>();
            _resolver.Start(result =>
            {
                if (result.Status.StatusCode == StatusCode.OK)
                {
                    // TODO: A resolver could return multiple results. What should happen?
                    var endpoint = result.Addresses![0].EndPoint;

                    // TODO: Only the first result returned by the resolver is used.
                    _addressTcs.TrySetResult(result.Addresses![0]);
                }
            });
        }

        public Task<BalancerAddress> AddressTask => _addressTcs.Task;
    }
}