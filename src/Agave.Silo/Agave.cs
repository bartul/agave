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

        async Task IAgave.Plant()
        {
            _logger.LogInformation($"Planting the agave {_grainContext.GrainId}.");
            _storage.State.Current = AgaveBlossomState.Planted;

            _reminder = await _reminderRegistry.RegisterOrUpdateReminder(
                callingGrainId: _grainContext.GrainId,
                reminderName: nameof(TimeToGerminateArrived),
                dueTime: TimeSpan.FromSeconds(5),
                period: TimeSpan.FromHours(24));

            await _storage.WriteStateAsync();
        }

        private async Task TimeToGerminateArrived()
        {
            int decision = _random.Next(1, 3);
            _logger.LogInformation($"Agave {_grainContext.GrainId} is deciding to germinate or die. Decision: {decision}.");

            if(decision == 1)
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
    public record AgaveState
    {
        public AgaveBlossomState Current { get; set; }
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
    internal record AgavePlanted(Guid AgaveId)
    {
        [Id(0)]
        public Guid AgaveId { get; init; } = AgaveId;
    }

    [Alias("Agave.IAgave")]
    public interface IAgave : IGrainWithGuidKey, IRemindable
    {
        [Alias("Plant")]
        Task Plant();
    }
}
