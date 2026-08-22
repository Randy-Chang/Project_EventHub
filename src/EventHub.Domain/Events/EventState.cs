namespace EventHub.Domain.Events;

public enum EventState
{
    Draft,
    Ready,
    Running,
    Ended,
    Cancelled
}
