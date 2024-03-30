using Agave.OrleansExtensions;
using Orleans.Timers;

namespace Agave;


[GrainType("agave")]
public sealed class Agave(
    [PersistentState("agave_ecosystem_store", "agave")] IPersistentState<AgaveState> storage,
    IGrainContext grainContext,
    IReminderRegistry reminderRegistry,
    ILogger<Agave> logger) : IAgave, IForcePersistance
{
    private readonly ILogger _logger = logger;
    private readonly IPersistentState<AgaveState> _storage = storage;
    private readonly IGrainContext _grainContext = grainContext;
    private readonly IReminderRegistry _reminderRegistry = reminderRegistry;
    private readonly Random _random = new(DateTime.Now.Millisecond);

    async Task IAgave.Plant(PlantSeedCommand plantSeedCommand)
    {
        _storage.State.Current = AgaveBlossomState.Planted;
        _storage.State.TimeToGerminate = plantSeedCommand.TimeToGerminate;
        _storage.State.SuccessRate = plantSeedCommand.SuccessRate;
        _storage.State.DegenerationRate = plantSeedCommand.DegenerationRate;
        _storage.State.TimeToBlossom = plantSeedCommand.TimeToBlossom;

        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);

        await _reminderRegistry.RegisterOrUpdateReminder(
            callingGrainId: _grainContext.GrainId,
            reminderName: nameof(TimeToGerminateArrived),
            dueTime: plantSeedCommand.TimeToGerminate);
    }

    public async Task TimeToGerminateArrived()
    {
        int decision = _random.Next(1, 3);
        _logger.AgaveGerminationDecision(_grainContext.GrainId, decision);

        if (decision == 1)
        {
            await Die();
        }
        else
        {
            await Germinate();
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

    public Task Blossom()
    {
        _storage.State.Current = AgaveBlossomState.Blossomed;
        _logger.AgaveBlossomingState(_grainContext.GrainId, _storage.State.Current, _storage.State);
        return Task.CompletedTask;
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
public record PlantSeedCommand(TimeSpan TimeToGerminate, double SuccessRate, double DegenerationRate, TimeSpan TimeToBlossom)
{
    [Id(0)]
    public TimeSpan TimeToGerminate { get; init; } = TimeToGerminate;
    [Id(1)]
    public double SuccessRate { get; init; } = SuccessRate;
    [Id(2)]
    public double DegenerationRate { get; init; } = DegenerationRate;
    [Id(3)]
    public TimeSpan TimeToBlossom { get; init; } = TimeToBlossom;
}

