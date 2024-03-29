namespace Agave;

internal static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Agave {GrainId} is deciding to germinate or die. Decision: {Decision}.")]
    public static partial void AgaveGerminationDecision(this ILogger logger, GrainId grainId, int decision);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Agave {GrainId} blossoming state - {state}.")]
    public static partial void AgaveBlossomingState(this ILogger logger, GrainId grainId, AgaveBlossomState state, [LogProperties(OmitReferenceName=true)] AgaveState snapshot); 

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Agave {GrainId} received reminder - {reminderName}.")]
    public static partial void ReminderArrived(this ILogger logger, GrainId grainId, string reminderName);
}

