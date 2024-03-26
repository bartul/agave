namespace Orleans.Timers
{
    public static class ReminderRegistryExtensions 
    {
        public static async Task UnregisterReminderByName(this IReminderRegistry registry, GrainId callingGrainId, string reminderName)
        {
            await registry.UnregisterReminder(callingGrainId, (IGrainReminder?)await registry.GetReminder(callingGrainId, reminderName));
        }
    }
}
