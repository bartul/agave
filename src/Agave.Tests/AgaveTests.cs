using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Agave.Tests;

public partial class AgaveTests
{
    private readonly ILogger<Agave> _logger;

    public AgaveTests(ITestOutputHelper output)
    {
        _logger = new LoggerFactory([new XunitLoggerProvider(output)]).CreateLogger<Agave>();
    }

    [Fact]
    public async void WhenPlantCommandIsExecuted_ThenAgaveIsPlanted()
    {
        var state = new AgaveState();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), new TestReminderRegistry(), _logger);

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromSeconds(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromSeconds(10)));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void GivenPlanted_WhenTimeToGerminateExpires_ThenAgaveIsGerminatedOrDead()
    {
        var state = new AgaveState();

        var reminderRegistry = new TestReminderRegistry();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromSeconds(10)));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(6));

        Assert.Contains(new[] { state.Current }, item => item == AgaveBlossomState.Germinated || item == AgaveBlossomState.Dead);
    }

    [Fact]
    public async void GivenPlanted_WhenTimeToGerminateHasNotPassed_ThenAgaveIsStillPlanted()
    {
        var state = new AgaveState();

        var reminderRegistry = new TestReminderRegistry();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromDays(10)));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(2));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void GivenGerminated_WhenTimeToBlossomExpires_ThenAgaveIsBlossomed()
    {
        var state = new AgaveState() { 
            Current = AgaveBlossomState.Planted, 
            TimeToGerminate = TimeSpan.FromDays(5), 
            SuccessRate = 1,
            DegenerationRate = 0.1,
            TimeToBlossom = TimeSpan.FromDays(10) 
        };

        var reminderRegistry = new TestReminderRegistry();
        Agave agave = new (new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await (agave as IRemindable).ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Germinate();
        reminderRegistry.AdvanceTime(TimeSpan.FromDays(11));

        Assert.Equal(AgaveBlossomState.Blossomed, state.Current);
    }

    [Fact]
    public async void GivenGerminated_WhenTimeToBlossomHasNotPassed_ThenAgaveIsStillGerminated()
    {
        var state = new AgaveState() { 
            Current = AgaveBlossomState.Planted, 
            TimeToGerminate = TimeSpan.FromDays(5), 
            SuccessRate = 1,
            DegenerationRate = 0.1,
            TimeToBlossom = TimeSpan.FromDays(10) 
        };

        var reminderRegistry = new TestReminderRegistry();
        Agave agave = new (new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await (agave as IRemindable).ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Germinate();

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(6));

        Assert.Equal(AgaveBlossomState.Germinated, state.Current);
    }
}

