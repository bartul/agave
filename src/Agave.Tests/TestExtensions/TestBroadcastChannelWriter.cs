using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;

namespace Agave.Tests.TestExtensions;

public class TestBroadcastChannelWriter<T>(ILogger logger, Action<object>? onPublish = null) : IBroadcastChannelWriter<T>
{
    private readonly ILogger _logger = logger;
    private readonly Action<object> onPublish = onPublish ?? (_ => { });

    public Task Publish(T item)
    {
        _logger.LogInformation($"Publishing item {item?.GetType().Name} to broadcast channel.");
        onPublish(item ?? throw new ArgumentNullException(nameof(item)));
        return Task.CompletedTask;
    }
}

