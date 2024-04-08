using Agave.OrleansExtensions;

namespace Agave;

[GrainType("gardner")]
[ImplicitChannelSubscription("event-bus")]
public sealed class Gardner(
    IGrainContext grainContext,
    ILogger<Gardner> logger) : IGardner, IOnBroadcastChannelSubscribed
{
    private readonly ILogger _logger = logger;
    private readonly IGrainContext _grainContext = grainContext;

    public Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
    {
        return streamSubscription.Attach<SeedProduced>(e => {
            _logger.ReceiveSeedProducedEvent(_grainContext.GrainId);
            return Task.CompletedTask;
        }, error => {
            _logger.ErrorReceivingSeedProducedEvent(error, _grainContext.GrainId);
            return Task.CompletedTask;
        });
    }
}

[Alias("Agave.IGardner")]
public interface IGardner : IGrain
{
}