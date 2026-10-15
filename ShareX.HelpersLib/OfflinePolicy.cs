using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareX.HelpersLib;

// This is a build policy, not a user preference. Configuration cannot enable networking.
public static class OfflinePolicy
{
    public static bool IsOffline => true;

    public static void DenyNetwork()
    {
        throw new InvalidOperationException("Сетевые функции отключены в этой офлайн-сборке.");
    }

    public static HttpClient CreateBlockedHttpClient() => new(new BlockedHandler());

    private sealed class BlockedHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(new HttpRequestException("Network access is disabled in this offline build."));
    }
}
