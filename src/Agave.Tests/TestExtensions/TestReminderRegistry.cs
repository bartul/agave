using Orleans.Runtime;
using Orleans.Timers;

namespace Agave.Tests.TestExtensions;

internal class TestReminderRegistry : IReminderRegistry
{
    private readonly List<TestGrainReminder> _reminders = new();
    private DateTime _currentTime = DateTime.UtcNow;
    
    public event EventHandler<TestGrainReminder>? ReminderTicked;
    public void TriggerReminderTickedEvent(TestGrainReminder reminder)
    {
        ReminderTicked?.Invoke(this, reminder);
    }


    public void AdvanceTime(TimeSpan time)
    {
        _currentTime += time;
        foreach(var reminder in GetRemindersReadyToTick(_currentTime).ToList())
        {
            TriggerReminderTickedEvent(reminder);
            this.RegisterOrUpdateReminder(reminder.GrainId, reminder.ReminderName, reminder.DueTime, reminder.Period, reminder.FirstTick, _currentTime + reminder.Period);
        };
        
    }
    private IEnumerable<TestGrainReminder> GetRemindersReadyToTick(DateTime currentTime)
    {
        return _reminders.OfType<TestGrainReminder>().Where(i => i.NextTick <= currentTime);
    }
    private TestGrainReminder RegisterOrUpdateReminder(GrainId callingGrainId, string reminderName, TimeSpan dueTime, TimeSpan period, DateTime firstTick, DateTime nextTick)
    {
        var reminder = _reminders.Where(i => i.GrainId == callingGrainId && i.ReminderName == reminderName)
            .SingleOrDefault();

        if (reminder != null)
        {
            _reminders.Remove(reminder);
        }
        reminder = new TestGrainReminder(callingGrainId, reminderName, dueTime, period, firstTick, nextTick);
        _reminders.Add(reminder);

        return reminder;
    }

    Task<IGrainReminder> IReminderRegistry.GetReminder(GrainId callingGrainId, string reminderName)
    {
        var reminder = _reminders.Where(i => i.GrainId == callingGrainId && i.ReminderName == reminderName)
            .OfType<IGrainReminder>()
            .Single();
        return Task.FromResult(reminder);
    }
    Task<List<IGrainReminder>> IReminderRegistry.GetReminders(GrainId callingGrainId)
    {
        return Task.FromResult(_reminders.Where(i => i.GrainId == callingGrainId).OfType<IGrainReminder>().ToList());
    }
    Task<IGrainReminder> IReminderRegistry.RegisterOrUpdateReminder(GrainId callingGrainId, string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        return Task.FromResult(this.RegisterOrUpdateReminder(callingGrainId, reminderName, dueTime, period, _currentTime + dueTime, _currentTime + dueTime) as IGrainReminder);
    }
    Task IReminderRegistry.UnregisterReminder(GrainId callingGrainId, IGrainReminder reminder)
    {
        _reminders.RemoveAll(i => i.GrainId == callingGrainId && i.ReminderName == reminder.ReminderName);
        return Task.CompletedTask;
    }
}

internal record TestGrainReminder(GrainId GrainId, string ReminderName, TimeSpan DueTime, TimeSpan Period, DateTime FirstTick, DateTime NextTick) : IGrainReminder;


