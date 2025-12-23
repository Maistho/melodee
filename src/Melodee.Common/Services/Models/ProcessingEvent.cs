namespace Melodee.Common.Services.Models;

public record ProcessingEvent(
    ProcessingEventType Type,
    string EventName,
    int Max,
    int Current,
    string Message,
    long BytesProcessed = 0,
    long TotalBytes = 0,
    int BatchSize = 0,
    int BatchCurrent = 0);
