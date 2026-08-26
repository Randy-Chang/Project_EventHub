using EventHub.Application.Events;
using EventHub.Application.Quizzes;
using EventHub.Domain.Events;
using EventHub.Domain.Quizzes;
using System.Reflection;

namespace EventHub.Application.Tests;

public sealed class HostSessionServiceTests
{
    [Fact]
    public void ActiveEventWithoutQuestionBank_DoesNotRecommendIrreversibleCompletion()
    {
        var resolveRecommendedAction = typeof(HostSessionService).GetMethod(
            "ResolveRecommendedAction",
            BindingFlags.NonPublic | BindingFlags.Static);
        var eventSummary = new EventSummary(
            Guid.NewGuid(),
            "Reusable Event",
            "ABC234",
            DateTimeOffset.UtcNow,
            EventState.Active,
            true);
        var quizState = new CurrentQuizState(
            QuizQuestionState.Waiting,
            null,
            null,
            null,
            null,
            [],
            null,
            null,
            0,
            0,
            0,
            null,
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        Assert.NotNull(resolveRecommendedAction);

        var action = resolveRecommendedAction.Invoke(null, [eventSummary, quizState, Array.Empty<QuestionBankSummary>()]);

        Assert.Equal(HostRecommendedAction.StartQuestion, action);
    }
}
