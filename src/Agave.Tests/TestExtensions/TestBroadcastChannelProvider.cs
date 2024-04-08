using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;

namespace Agave.Tests.TestExtensions;

public class TestBroadcastChannelProvider(ILogger logger, Action<object>? onPublish = null) : IBroadcastChannelProvider
{
    private readonly ILogger _logger = logger;
    private readonly Action<object>? onPublish = onPublish;

    public IBroadcastChannelWriter<T> GetChannelWriter<T>(ChannelId streamId)
    {
        return new TestBroadcastChannelWriter<T>(_logger, onPublish);
    }

}

