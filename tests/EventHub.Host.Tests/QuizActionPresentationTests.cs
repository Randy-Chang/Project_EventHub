using EventHub.Host;

namespace EventHub.Host.Tests;

public sealed class QuizActionPresentationTests
{
    [Theory]
    [InlineData((int)QuizState.Waiting, (int)QuizPrimaryAction.Start, "開始本題")]
    [InlineData((int)QuizState.Open, (int)QuizPrimaryAction.Close, "關閉作答")]
    [InlineData((int)QuizState.Closed, (int)QuizPrimaryAction.Reveal, "公布答案")]
    [InlineData((int)QuizState.Revealed, (int)QuizPrimaryAction.Next, "下一題")]
    public void Resolve_MapsServerStateToOnePrimaryAction(
        int stateValue,
        int expectedActionValue,
        string expectedButtonText)
    {
        var state = (QuizState)stateValue;
        var expectedAction = (QuizPrimaryAction)expectedActionValue;
        var result = QuizActionPresentation.Resolve(state, true, QuizQuestionMode.Scored, true, true);

        Assert.Equal(expectedAction, result.Action);
        Assert.Equal(expectedButtonText, result.ButtonText);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public void Resolve_WaitingWithoutQuestion_DisablesStart()
    {
        var result = QuizActionPresentation.Resolve(QuizState.Waiting, false, null, false, true);

        Assert.Equal(QuizPrimaryAction.None, result.Action);
        Assert.Equal("請先選擇題庫", result.ButtonText);
        Assert.False(result.IsEnabled);
    }

    [Fact]
    public void Resolve_LastScoredQuestion_OffersFinalLeaderboard()
    {
        var result = QuizActionPresentation.Resolve(
            QuizState.Revealed,
            true,
            QuizQuestionMode.Scored,
            false,
            true);

        Assert.Equal(QuizPrimaryAction.FinalLeaderboard, result.Action);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public void Resolve_LastPracticeQuestion_OffersStartOfficial()
    {
        var result = QuizActionPresentation.Resolve(
            QuizState.Revealed,
            true,
            QuizQuestionMode.Practice,
            false,
            true);

        Assert.Equal(QuizPrimaryAction.StartOfficial, result.Action);
        Assert.True(result.IsEnabled);
    }
}
