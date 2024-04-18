using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;
using Orleans.Runtime;
using Orleans.Streams;

namespace Agave.Tests;

public partial class AgaveTests(ITestOutputHelper output)
{
    private readonly ILogger<Agave> _logger = new LoggerFactory([new XunitLoggerProvider(output)]).CreateLogger<Agave>();

    [Fact]
    public async void WhenPlanted_ThenAgaveIsPlanted()
    {
        var state = new AgaveState();

        var reminderRegistry = new TestReminderRegistry();
        var serviceProvider = new TestServiceProvider(_logger);
        serviceProvider.AddKeyedService<IBroadcastChannelProvider>("event-bus", new TestBroadcastChannelProvider(_logger));
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(_logger, serviceProvider), reminderRegistry, _logger);

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromSeconds(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromSeconds(10), NumberOfSeedsProducing: 2));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void WhenPlantedAndTimeToGerminateExpires_ThenAgaveIsGerminatedOrDead()
    {
        var state = new AgaveState();
        var reminderRegistry = new TestReminderRegistry();
        var serviceProvider = new TestServiceProvider(_logger);
        serviceProvider.AddKeyedService<IBroadcastChannelProvider>("event-bus", new TestBroadcastChannelProvider(_logger));
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(_logger, serviceProvider), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromSeconds(10), NumberOfSeedsProducing: 2));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(6));

        Assert.Contains(new[] { state.Current }, item => item == AgaveBlossomState.Germinated || item == AgaveBlossomState.Dead);
    }

    [Fact]
    public async void WhenPlantedTimeToGerminateHasNotPassed_ThenAgaveIsStillPlanted()
    {
        var state = new AgaveState();

        var reminderRegistry = new TestReminderRegistry();
        var serviceProvider = new TestServiceProvider(_logger);
        serviceProvider.AddKeyedService<IBroadcastChannelProvider>("event-bus", new TestBroadcastChannelProvider(_logger));
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(_logger, serviceProvider), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await agave.ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Plant(new PlantSeedCommand(TimeToGerminate: TimeSpan.FromDays(5), SuccessRate: 1, DegenerationRate: 0.1, TimeToBlossom: TimeSpan.FromDays(10), NumberOfSeedsProducing: 2));

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(2));

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void GivenPlanted_WhenGerminatedAndWhenTimeToBlossomExpires_ThenAgaveIsBlossomed_AndSeedProducePublished()
    {
        var state = new AgaveState() { 
            Current = AgaveBlossomState.Planted, 
            TimeToGerminate = TimeSpan.FromDays(5), 
            SuccessRate = 1,
            DegenerationRate = 0.1,
            TimeToBlossom = TimeSpan.FromDays(10),
            NumberOfSeedsProducing = 2
        };

        var reminderRegistry = new TestReminderRegistry();
        var publishedEvents = new List<object>();
        var serviceProvider = new TestServiceProvider(_logger);
        serviceProvider.AddKeyedService<IBroadcastChannelProvider>("event-bus", new TestBroadcastChannelProvider(_logger, publishedEvents.Add));
        Agave agave = new (new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(_logger, serviceProvider), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await (agave as IRemindable).ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Germinate();
        reminderRegistry.AdvanceTime(TimeSpan.FromDays(11));

        Assert.Equal(AgaveBlossomState.Blossomed, state.Current);
        Assert.Equal(publishedEvents.OfType<SeedProduced>().Count(), state.NumberOfSeedsProducing);
        Assert.Collection(publishedEvents.OfType<SeedProduced>(),
            seed => {
                Assert.Equal(TimeSpan.FromDays(5), seed.TimeToGerminate);
                Assert.Equal(0.9, seed.SuccessRate);
                Assert.Equal(0.1, seed.DegenerationRate);
                Assert.Equal(TimeSpan.FromDays(10), seed.TimeToBlossom);
                Assert.Equal(2, seed.NumberOfSeedsProducing);
            },
            seed => {
                Assert.Equal(TimeSpan.FromDays(5), seed.TimeToGerminate);
                Assert.Equal(0.9, seed.SuccessRate);
                Assert.Equal(0.1, seed.DegenerationRate);
                Assert.Equal(TimeSpan.FromDays(10), seed.TimeToBlossom);
                Assert.Equal(2, seed.NumberOfSeedsProducing);
            });
    }

    [Fact]
    public async void GivenPlanted_WhenGerminatedAndTimeToBlossomHasNotPassed_ThenAgaveIsStillGerminated()
    {
        var state = new AgaveState() { 
            Current = AgaveBlossomState.Planted, 
            TimeToGerminate = TimeSpan.FromDays(5), 
            SuccessRate = 1,
            DegenerationRate = 0.1,
            TimeToBlossom = TimeSpan.FromDays(10) 
        };

        var reminderRegistry = new TestReminderRegistry();
        var serviceProvider = new TestServiceProvider(_logger);
        serviceProvider.AddKeyedService<IBroadcastChannelProvider>("event-bus", new TestBroadcastChannelProvider(_logger));
        Agave agave = new (new TestPersistentState<AgaveState>(state), new TestGrainContext<Agave>(_logger, serviceProvider), reminderRegistry, _logger);
        reminderRegistry.ReminderTicked += async (_, reminder) => await (agave as IRemindable).ReceiveReminder(reminder.ReminderName, new TickStatus(reminder.FirstTick, reminder.Period, reminder.NextTick));

        await agave.Germinate();

        reminderRegistry.AdvanceTime(TimeSpan.FromDays(6));

        Assert.Equal(AgaveBlossomState.Germinated, state.Current);
    }
}

