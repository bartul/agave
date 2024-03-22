using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Timers;

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
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext(), new TestReminderRegistry(), _loggerFactory);
        await agave.Plant();

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }

    [Fact]
    public async void GivenPlanted_When10SecondsPasses_ThenAgaveIsGerminated()
    {
        var state = new AgaveState();
        var reminderRegistry = new TestReminderRegistry();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext(), reminderRegistry, _loggerFactory);
        await agave.Plant();

        reminderRegistry.AdvanceTime(TimeSpan.FromSeconds(6));

        Assert.Equal(AgaveBlossomState.Germinated, state.Current);
    }
}

internal class TestReminderRegistry : IReminderRegistry
{
    private readonly Dictionary<GrainId, List<IGrainReminder>> _reminders = new();
    private readonly IReminderRegistry _reminderRegistry;
    private DateTime _currentTime = DateTime.UtcNow;

    public void AdvanceTime(TimeSpan time)
    {
        _currentTime += time;
    }

    public TestReminderRegistry()
    {
        _reminderRegistry = this;
    }
    async Task<IGrainReminder> IReminderRegistry.GetReminder(GrainId callingGrainId, string reminderName)
    {
        var grainReminders = await _reminderRegistry.GetReminders(callingGrainId);
        return grainReminders.Single(reminder => reminder.ReminderName == reminderName);
    }
    Task<List<IGrainReminder>> IReminderRegistry.GetReminders(GrainId callingGrainId)
    {
        return Task.FromResult(_reminders.GetValueOrDefault(callingGrainId) ?? []);
    }
    Task<IGrainReminder> IReminderRegistry.RegisterOrUpdateReminder(GrainId callingGrainId, string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        if (!_reminders.ContainsKey(callingGrainId))
        {
            _reminders[callingGrainId] = [];
        }
        _reminders[callingGrainId].RemoveAll(x => x.ReminderName == reminderName);

        var reminder = new TestGrainReminder(reminderName, dueTime, period);
        _reminders[callingGrainId].Add(reminder);
        return Task.FromResult<IGrainReminder>(reminder);
    }
    Task IReminderRegistry.UnregisterReminder(GrainId callingGrainId, IGrainReminder reminder)
    {
        _reminders[callingGrainId]?.RemoveAll(x => x.ReminderName == reminder.ReminderName);
        return Task.CompletedTask;
    }
}
internal class TestGrainReminder : IGrainReminder
{
    private readonly string _reminderName;
    private readonly TimeSpan _dueTime;
    private readonly TimeSpan _period;

    public TestGrainReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        _reminderName = reminderName;
        _dueTime = dueTime;
        _period = period;
    }
    string IGrainReminder.ReminderName => _reminderName;
}

