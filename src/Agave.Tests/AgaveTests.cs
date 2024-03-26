using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Agave.Tests;

public partial class AgaveTests
{
    private readonly LoggerFactory _loggerFactory;

    public AgaveTests(ITestOutputHelper output)
    {
        _loggerFactory = new LoggerFactory([new XunitLoggerProvider(output)]);
    }

    [Fact]
    public async void WhenPlantCommandIsExecuted_ThenAgaveIsPlanted()
    {
        var state = new AgaveState();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), new TestReminderRegistry(), _loggerFactory);
        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromSeconds(5), SuccessRate: 1, DegenerationRate: 0.1));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void GivenPlanted_WhenTimeToGerminateExpires_ThenAgaveIsGerminatedOrDead()
    {
        var state = new AgaveState();
        var reminderRegistry = new TestReminderRegistry();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _loggerFactory);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));
        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(6));

        Assert.Contains(new[] { state.Current }, item => item == AgaveBlossomState.Germinated || item == AgaveBlossomState.Dead);
    }
    [Fact]
    public async void GivenPlanted_WhenTimeToGerminateHasNotPassed_ThenAgaveIsStillPlanted()
    {
        var state = new AgaveState();
        var reminderRegistry = new TestReminderRegistry();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(), reminderRegistry, _loggerFactory);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));
        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(2));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }
}

