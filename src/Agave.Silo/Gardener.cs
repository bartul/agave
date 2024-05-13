namespace Agave;

[GrainType("gardner")]
[ImplicitChannelSubscription("event-bus")]
public sealed class Gardner(
    IGrainContext grainContext,
    IGrainFactory grainFactory,
    ILogger<Gardner> logger) : IGardner, IOnBroadcastChannelSubscribed
{
    private readonly ILogger _logger = logger;
    private readonly IGrainContext _grainContext = grainContext;
    private readonly IGrainFactory _grainFactory = grainFactory;

    public Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
    {
        return streamSubscription.Attach<SeedProduced>(e => {
            _logger.ReceiveSeedProducedEvent(_grainContext.GrainId);
            _grainFactory.GetGrain<IAgave>(Guid.NewGuid()).Plant(new PlantSeedCommand(
                TimeToGerminate: e.TimeToGerminate, 
                SuccessRate: e.SuccessRate, 
                DegenerationRate: e.DegenerationRate,
                TimeToBlossom: e.TimeToBlossom,
                NumberOfSeedsProducing: e.NumberOfSeedsProducing));
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