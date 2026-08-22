using EventHub.Domain.Common;
using EventHub.Domain.Events;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Domain.Tests;

public sealed class EventTests
{
    [Fact]
    public void Create_NormalizesNameAndOpensJoin()
    {
        var eventItem = DomainEvent.Create(
            "  年終晚會  ",
            new DateTimeOffset(2026, 12, 18, 10, 0, 0, TimeSpan.Zero),
            "credential-hash",
            new DateTimeOffset(2026, 8, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("年終晚會", eventItem.Name);
        Assert.Equal(EventState.Draft, eventItem.State);
        Assert.True(eventItem.IsJoinOpen);
    }

    [Fact]
    public void End_ClosesJoinAndPreventsReopening()
    {
        var eventItem = DomainEvent.Create(
            "家庭日",
            DateTimeOffset.UtcNow,
            "credential-hash",
            DateTimeOffset.UtcNow);
        eventItem.Start();

        eventItem.End();

        Assert.Equal(EventState.Ended, eventItem.State);
        Assert.False(eventItem.IsJoinOpen);
        Assert.Throws<DomainValidationException>(() => eventItem.SetJoinOpen(true));
    }
}
