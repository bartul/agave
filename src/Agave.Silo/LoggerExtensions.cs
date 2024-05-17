namespace Agave;

internal static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Agave {grainId} is deciding to germinate. Decision: {decision}.")]
    public static partial void AgaveGerminationDecision(this ILogger logger, GrainId grainId, bool decision);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Agave {grainId} blossoming state - {state}.")]
    public static partial void AgaveBlossomingState(this ILogger logger, GrainId grainId, AgaveBlossomState state, [LogProperties] AgaveState grainState); 

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Agave {grainId} producing a seed.")]
    public static partial void AgaveProducingSeed(this ILogger logger, GrainId grainId, [LogProperties] AgaveState grainState); 

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Agave {GrainId} received reminder - {reminderName}.")]
    public static partial void ReminderArrived(this ILogger logger, GrainId grainId, string reminderName);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Agave {grainId} state saved.")]
    public static partial void StateSaved(this ILogger logger, GrainId grainId, [LogProperties] AgaveState grainState);


    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Gardner {grainId} received seed produced event.")]
    public static partial void ReceiveSeedProducedEvent(this ILogger logger, GrainId grainId);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Error, Message = "Gardner {grainId} experienced an error on receiving seed produced event.")]
    public static partial void ErrorReceivingSeedProducedEvent(this ILogger logger, Exception ex, GrainId grainId);
}

