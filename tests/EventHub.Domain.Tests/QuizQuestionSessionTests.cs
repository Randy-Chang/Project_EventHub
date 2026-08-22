using EventHub.Domain.Common;
using EventHub.Domain.Quizzes;

namespace EventHub.Domain.Tests;

public sealed class QuizQuestionSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StateFlow_WaitingToOpenToClosedToRevealed()
    {
        var session = CreateSession();

        session.Open(Now, TimeSpan.FromSeconds(20));
        session.Close(Now.AddSeconds(10));
        session.Reveal(Now.AddSeconds(11));

        Assert.Equal(QuizQuestionState.Revealed, session.State);
        Assert.Equal(Now, session.StartedAtUtc);
        Assert.Equal(Now.AddSeconds(20), session.AnswerDeadlineUtc);
        Assert.Equal(Now.AddSeconds(10), session.ClosedAtUtc);
        Assert.Equal(Now.AddSeconds(11), session.RevealedAtUtc);
    }

    [Fact]
    public void CloseIfDeadlinePassed_ClosesAtServerTime()
    {
        var session = CreateSession();
        session.Open(Now, TimeSpan.FromSeconds(10));

        Assert.False(session.CloseIfDeadlinePassed(Now.AddSeconds(9)));
        Assert.True(session.CloseIfDeadlinePassed(Now.AddSeconds(10)));
        Assert.Equal(QuizQuestionState.Closed, session.State);
    }

    [Fact]
    public void Reveal_RejectsOpenSession()
    {
        var session = CreateSession();
        session.Open(Now, TimeSpan.FromSeconds(10));

        Assert.Throws<DomainValidationException>(() => session.Reveal(Now));
    }

    private static QuizQuestionSession CreateSession() =>
        QuizQuestionSession.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
}
