using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;
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
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), new TestGrainContext(), new TestReminderRegistry(), _loggerFactory);
        await agave.Plant();

        await Task.Delay(TimeSpan.FromSeconds(6));

        Assert.Equal(AgaveBlossomState.Germinated, state.Current);
    }

}

internal class TestGrainContext : IGrainContext
{
    public GrainReference GrainReference => throw new NotImplementedException();

    public GrainId GrainId
    {

        get => GrainId.Create("Agave", Guid.NewGuid().ToString());

    }

    public object GrainInstance => throw new NotImplementedException();

    public ActivationId ActivationId => throw new NotImplementedException();

    public GrainAddress Address => throw new NotImplementedException();

    public IServiceProvider ActivationServices => throw new NotImplementedException();

    public IGrainLifecycle ObservableLifecycle => throw new NotImplementedException();

    public IWorkItemScheduler Scheduler => throw new NotImplementedException();

    public Task Deactivated => throw new NotImplementedException();

    public void Activate(Dictionary<string, object> requestContext, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public void Deactivate(DeactivationReason deactivationReason, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IGrainContext? other)
    {
        throw new NotImplementedException();
    }

    public TComponent GetComponent<TComponent>() where TComponent : class
    {
        throw new NotImplementedException();
    }

    public TTarget GetTarget<TTarget>() where TTarget : class
    {
        throw new NotImplementedException();
    }

    public void Migrate(Dictionary<string, object> requestContext, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public void ReceiveMessage(object message)
    {
        throw new NotImplementedException();
    }

    public void Rehydrate(IRehydrationContext context)
    {
        throw new NotImplementedException();
    }

    public void SetComponent<TComponent>(TComponent value) where TComponent : class
    {
        throw new NotImplementedException();
    }
}

internal class TestReminderRegistry : IReminderRegistry
{
    private readonly Dictionary<GrainId, List<IGrainReminder>> _reminders = new();

    Task<IGrainReminder> IReminderRegistry.GetReminder(GrainId callingGrainId, string reminderName)
    {
        return Task.FromResult<IGrainReminder>(new TestGrainReminder(reminderName, TimeSpan.Zero, TimeSpan.Zero));
    }

    Task<List<IGrainReminder>> IReminderRegistry.GetReminders(GrainId callingGrainId)
    {
        return Task.FromResult(_reminders.GetValueOrDefault(callingGrainId) ?? new List<IGrainReminder>());
    }

    Task<IGrainReminder> IReminderRegistry.RegisterOrUpdateReminder(GrainId callingGrainId, string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        return Task.FromResult<IGrainReminder>(new TestGrainReminder(reminderName, dueTime, period));
    }

    Task IReminderRegistry.UnregisterReminder(GrainId callingGrainId, IGrainReminder reminder)
    {
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

