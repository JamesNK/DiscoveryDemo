namespace Discovery.Tests;

internal sealed class TestHandler : HttpMessageHandler
{
    public HttpRequestMessage? RequestMessage { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestMessage = request;
        return Task.FromResult(new HttpResponseMessage());
    }
}