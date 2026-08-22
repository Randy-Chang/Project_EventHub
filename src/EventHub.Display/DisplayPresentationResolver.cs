namespace EventHub.Display;

public static class DisplayPresentationResolver
{
    public static DisplayPresentationState Resolve(CurrentDisplayStateDto state)
    {
        return state.Mode switch
        {
            DisplayMode.Waiting => DisplayPresentationState.Joining,
            DisplayMode.Question when state.Question?.State == QuizQuestionState.Closed =>
                DisplayPresentationState.QuestionClosed,
            DisplayMode.Question => DisplayPresentationState.QuestionOpen,
            DisplayMode.Result => DisplayPresentationState.AnswerRevealed,
            DisplayMode.Leaderboard => DisplayPresentationState.Leaderboard,
            _ => DisplayPresentationState.WaitingForEvent
        };
    }
}
