using Agave.OrleansExtensions;

namespace Agave;


[GrainType("agave")]
public sealed class Agave(
    [PersistentState("agave_ecosystem_store", "agave")] IPersistentState<AgaveState> storage,
    IGrainContext grainContext,
    IReminderRegistry reminderRegistry,
    ILogger<Agave> logger) : IAgave, IForcePersistence
{
    private readonly ILogger _logger = logger;
    private readonly IPersistentState<AgaveState> _storage = storage;
    private readonly IGrainContext _grainContext = grainContext;
    private readonly IReminderRegistry _reminderRegistry = reminderRegistry;
    private readonly IBroadcastChannelProvider _broadcastChannelProvider = grainContext.ActivationServices.GetRequiredKeyedService<IBroadcastChannelProvider>("event-bus");
    private readonly Random _random = new(DateTime.Now.Millisecond);

    async Task IAgave.Plant(PlantSeedCommand plantSeedCommand)
    {
        _storage.State.Current = AgaveBlossomState.Planted;
        _storage.State.TimeToGerminate = plantSeedCommand.TimeToGerminate;
        _storage.State.SuccessRate = plantSeedCommand.SuccessRate;
        _storage.State.DegenerationRate = plantSeedCommand.DegenerationRate;
        _storage.State.TimeToBlossom = plantSeedCommand.TimeToBlossom;
        _storage.State.NumberOfSeedsProducing = plantSeedCommand.NumberOfSeedsProducing;

        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);

        await _reminderRegistry.RegisterOrUpdateReminder(
            callingGrainId: _grainContext.GrainId,
            reminderName: nameof(TimeToGerminateArrived),
            dueTime: plantSeedCommand.TimeToGerminate);
    }

    public async Task TimeToGerminateArrived()
    {
        var decision = _random.Next(1, 100) <= _storage.State.SuccessRate * 100;
        _logger.AgaveGerminationDecision(_grainContext.GrainId, decision);

        if (decision)
        {
            await Germinate();
        }
        else
        {
            await Die();
        }
    }

    public async Task Germinate()
    {
        _storage.State.Current = AgaveBlossomState.Germinated;
        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);

        await _reminderRegistry.RegisterOrUpdateReminder(
            callingGrainId: _grainContext.GrainId,
            reminderName: nameof(TimeToBlossomArrived),
            dueTime: _storage.State.TimeToBlossom);
    }

    public async Task TimeToBlossomArrived() => await Blossom();

    public async Task Blossom()
    {
        var channelWriter = _broadcastChannelProvider.GetChannelWriter<SeedProduced>(ChannelId.Create("event-bus", Guid.Empty));
        for (int i = 0; i < _storage.State.NumberOfSeedsProducing; i++)
        { 
            _logger.AgaveProducingSeed(_grainContext.GrainId, _storage.State);
            await channelWriter.Publish(new SeedProduced(_storage.State.TimeToGerminate, _storage.State.SuccessRate - _storage.State.DegenerationRate, _storage.State.DegenerationRate, _storage.State.TimeToBlossom, _storage.State.NumberOfSeedsProducing));
        }

        _storage.State.Current = AgaveBlossomState.Dead;
        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);
    }

    public Task Die()
    {
        _storage.State.Current = AgaveBlossomState.Dead;
        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);
        return Task.CompletedTask;
    }

    async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.ReminderArrived(_grainContext.GrainId, reminderName);
        switch (reminderName)
        {
            case nameof(TimeToGerminateArrived): await TimeToGerminateArrived(); break;
            case nameof(TimeToBlossomArrived): await TimeToBlossomArrived(); break;
        }
        await _reminderRegistry.UnregisterReminderByName(_grainContext.GrainId, reminderName);
    }

    public async Task WriteState()
    {
        _logger.StateSaved(_grainContext.GrainId, _storage.State);
        await _storage.WriteStateAsync();
    }
}

[GenerateSerializer, Immutable]
[Alias("Agave.AgaveState")]
public record AgaveState()
{
    [Id(0)]
    public AgaveBlossomState Current { get; set; } = AgaveBlossomState.Planted;
    [Id(1)]
    public TimeSpan TimeToGerminate { get; set; } = TimeSpan.Zero;
    [Id(2)]
    public double SuccessRate { get; set; } = 0;
    [Id(3)]
    public double DegenerationRate { get; set; } = 0;
    [Id(4)]
    public TimeSpan TimeToBlossom { get; set; } = TimeSpan.Zero;
    [Id(5)]
    public int NumberOfSeedsProducing { get; set; } = 1;
}

[GenerateSerializer]
public enum AgaveBlossomState
{
    Planted,
    Germinated,
    Blossomed,
    Dead
}

[GenerateSerializer, Immutable]
[Alias("Agave.AgavePlanted")]
internal record AgavePlanted(Guid AgaveId, TimeSpan TimeToGerminate, double SuccessRate, double DegenerationRate)
{
    [Id(0)]
    public Guid AgaveId { get; init; } = AgaveId;
    [Id(1)]
    public TimeSpan TimeToGerminate { get; init; } = TimeToGerminate;
    [Id(2)]
    public double SuccessRate { get; init; } = SuccessRate;
    [Id(3)]
    public double DegenerationRate { get; init; } = DegenerationRate;
}

[Alias("Agave.IAgave")]
public interface IAgave : IGrainWithGuidKey, IRemindable
{
    [Alias("Plant")]
    Task Plant(PlantSeedCommand command);
}

[GenerateSerializer, Immutable]
[Alias("Agave.PlantSeedCommand")]
public record PlantSeedCommand(TimeSpan TimeToGerminate, double SuccessRate, double DegenerationRate, TimeSpan TimeToBlossom, int NumberOfSeedsProducing)
{
    [Id(0)]
    public TimeSpan TimeToGerminate { get; init; } = TimeToGerminate;
    [Id(1)]
    public double SuccessRate { get; init; } = SuccessRate;
    [Id(2)]
    public double DegenerationRate { get; init; } = DegenerationRate;
    [Id(3)]
    public TimeSpan TimeToBlossom { get; init; } = TimeToBlossom;
    [Id(4)]
    public int NumberOfSeedsProducing { get; init; } = NumberOfSeedsProducing;
}

[GenerateSerializer, Immutable]
[Alias("Agave.SeedProduced")]
public record SeedProduced(TimeSpan TimeToGerminate, double SuccessRate, double DegenerationRate, TimeSpan TimeToBlossom, int NumberOfSeedsProducing)
{
    [Id(0)]
    public TimeSpan TimeToGerminate { get; init; } = TimeToGerminate;
    [Id(1)]
    public double SuccessRate { get; init; } = SuccessRate;
    [Id(2)]
    public double DegenerationRate { get; init; } = DegenerationRate;
    [Id(3)]
    public TimeSpan TimeToBlossom { get; init; } = TimeToBlossom;
    [Id(4)]
    public int NumberOfSeedsProducing { get; init; } = NumberOfSeedsProducing;
}