namespace Orleans.Timers
{
    public static class ReminderRegistryExtensions 
    {
        public static async Task UnregisterReminderByName(this IReminderRegistry registry, GrainId callingGrainId, string reminderName)
        {
            await registry.UnregisterReminder(callingGrainId, (IGrainReminder?)await registry.GetReminder(callingGrainId, reminderName));
        }
        public static async Task<IGrainReminder> RegisterOrUpdateReminder(this IReminderRegistry registry, GrainId callingGrainId, string reminderName, TimeSpan dueTime)
        {
            return await registry.RegisterOrUpdateReminder(callingGrainId, reminderName, dueTime, TimeSpan.FromMilliseconds(4294967294));
        }
    }
}
