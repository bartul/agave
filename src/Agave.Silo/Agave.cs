using Orleans.Timers;

namespace Agave

{
    [GrainType("agave")]
    public sealed class Agave : IAgave
    {
        private readonly ILogger _logger;
        private readonly IPersistentState<AgaveState> _storage;
        private readonly IGrainContext _grainContext;
        private readonly IReminderRegistry _reminderRegistry;
        private IGrainReminder? _reminder;
        private readonly Random _random = new(DateTime.Now.Millisecond);

        public Agave(
            [PersistentState("agave_ecosystem_store", "agave")] IPersistentState<AgaveState> storage,
            IGrainContext grainContext,
            IReminderRegistry reminderRegistry,
            ILoggerFactory loggerFactory)
        {
            _storage = storage;
            _grainContext = grainContext;
            _reminderRegistry = reminderRegistry;
            _logger = loggerFactory.CreateLogger<Agave>();
        }

        async Task IAgave.Plant(PlantSeedCommand plantSeedCommand)
        {
            _logger.LogInformation("Planting the agave {GrainId}.", _grainContext.GrainId);
            _storage.State.Current = AgaveBlossomState.Planted;
            _storage.State.TimeToGerminate = plantSeedCommand.TimeToGerminate;
            _storage.State.SuccessRate = plantSeedCommand.SuccessRate;
            _storage.State.DegenerationRate = plantSeedCommand.DegenerationRate;

            _reminder = await _reminderRegistry.RegisterOrUpdateReminder(
                callingGrainId: _grainContext.GrainId,
                reminderName: nameof(TimeToGerminateArrived),
                dueTime: plantSeedCommand.TimeToGerminate,
                period: TimeSpan.FromHours(24));

            await _storage.WriteStateAsync();
        }

        private async Task TimeToGerminateArrived()
        {
            int decision = _random.Next(1, 3);
            _logger.LogInformation($"Agave {_grainContext.GrainId} is deciding to germinate or die. Decision: {decision}.");

            if (decision == 1)
            {
                await Dead();
            }
            else
            {
                await Germinate();
            }
        }

        private async Task Germinate()
        {
            _logger.LogInformation($"Agave {_grainContext.GrainId} germinated.");
            _storage.State.Current = AgaveBlossomState.Germinated;

            await _storage.WriteStateAsync();
        }
        private async Task Dead()
        {
            _logger.LogInformation($"Agave {_grainContext.GrainId} died.");
            _storage.State.Current = AgaveBlossomState.Dead;

            await _storage.WriteStateAsync();
        }

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            _logger.LogInformation($"Got reminder {reminderName}.");
            if (reminderName == nameof(TimeToGerminateArrived))
            {
                await TimeToGerminateArrived();
                await _reminderRegistry.UnregisterReminderByName(_grainContext.GrainId, reminderName);
            }
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
    public record PlantSeedCommand(TimeSpan TimeToGerminate, double SuccessRate, double DegenerationRate)
    {
        [Id(0)]
        public TimeSpan TimeToGerminate { get; init; } = TimeToGerminate;
        [Id(1)]
        public double SuccessRate { get; init; } = SuccessRate;
        [Id(2)]
        public double DegenerationRate { get; init; } = DegenerationRate;
    }
}
