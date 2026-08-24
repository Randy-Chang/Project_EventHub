using EventHub.Domain.Common;
using EventHub.Domain.Events;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Domain.Tests;

public sealed class EventTests
{
    [Fact]
    public void Create_NormalizesNameAndStartsAsClosedDraft()
    {
        var eventItem = DomainEvent.Create(
            "  年終晚會  ",
            "8K3F2A",
            new DateTimeOffset(2026, 12, 18, 10, 0, 0, TimeSpan.Zero),
            "credential-hash",
            new DateTimeOffset(2026, 8, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("年終晚會", eventItem.Name);
        Assert.Equal("8K3F2A", eventItem.JoinCode);
        Assert.Equal(EventState.Draft, eventItem.State);
        Assert.Equal(DisplayMode.Waiting, eventItem.DisplayMode);
        Assert.False(eventItem.IsJoinOpen);
    }

    [Fact]
    public void SetDisplayMode_UpdatesPresentationMode()
    {
        var eventItem = DomainEvent.Create(
            "家庭日",
            "ABC234",
            DateTimeOffset.UtcNow,
            "credential-hash",
            DateTimeOffset.UtcNow);

        eventItem.SetDisplayMode(DisplayMode.Leaderboard);

        Assert.Equal(DisplayMode.Leaderboard, eventItem.DisplayMode);
    }

    [Theory]
    [InlineData(" 8k3f2a ", "8K3F2A")]
    [InlineData("abc234", "ABC234")]
    public void JoinCode_NormalizesTrimAndCase(string input, string expected)
    {
        Assert.Equal(expected, EventJoinCode.Normalize(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC23")]
    [InlineData("ABC230")]
    [InlineData("ABC23I")]
    [InlineData("ABC 23")]
    public void JoinCode_RejectsInvalidFormat(string input)
    {
        Assert.Throws<DomainValidationException>(() => EventJoinCode.Normalize(input));
    }

    [Fact]
    public void Lifecycle_AdvancesInOrderAndCompletedPreventsReopening()
    {
        var eventItem = DomainEvent.Create(
            "家庭日",
            "ABC234",
            DateTimeOffset.UtcNow,
            "credential-hash",
            DateTimeOffset.UtcNow);
        eventItem.MarkReady();
        eventItem.SetJoinOpen(true);
        eventItem.Activate();

        eventItem.Complete();

        Assert.Equal(EventState.Completed, eventItem.State);
        Assert.False(eventItem.IsJoinOpen);
        Assert.Throws<DomainValidationException>(() => eventItem.SetJoinOpen(true));
    }

    [Fact]
    public void Lifecycle_RejectsSkippedAndReverseTransitions()
    {
        var eventItem = DomainEvent.Create(
            "家庭日",
            "ABC234",
            DateTimeOffset.UtcNow,
            "credential-hash",
            DateTimeOffset.UtcNow);

        Assert.Throws<DomainValidationException>(eventItem.Activate);
        eventItem.MarkReady();
        Assert.Throws<DomainValidationException>(eventItem.MarkReady);
        eventItem.Activate();
        Assert.Throws<DomainValidationException>(eventItem.Activate);
    }
}
