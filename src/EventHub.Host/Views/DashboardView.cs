namespace EventHub.Host.Views;

internal partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    public void Render(
        string eventName,
        string quizTitle,
        int participantCount,
        string questionPosition,
        QuizState quizState,
        HostConnectionState connectionState,
        DisplayMode displayMode,
        EventState eventState,
        HostRecommendedAction recommendedAction,
        int answeredCount,
        int onlineCount)
    {
        currentEventValueLabel.Text = $"{eventName} · {eventState}";
        currentQuizValueLabel.Text = quizTitle;
        participantCountValueLabel.Text = $"{onlineCount} 在線 / {participantCount} 人";
        questionValueLabel.Text = questionPosition;
        quizStateValueLabel.Text = $"{quizState} · 已作答 {answeredCount}/{participantCount}";
        serverValueLabel.Text = connectionState switch
        {
            HostConnectionState.Connected => "● Connected",
            HostConnectionState.Connecting => "● Connecting...",
            HostConnectionState.Reconnecting => "● Reconnecting...",
            _ => "● Disconnected"
        };
        displayValueLabel.Text = $"● {displayMode}";
        titleLabel.Text = $"活動控制台　下一步：{FormatRecommendedAction(recommendedAction)}";
    }

    private static string FormatRecommendedAction(HostRecommendedAction action) => action switch
    {
        HostRecommendedAction.PrepareEvent => "完成準備",
        HostRecommendedAction.OpenJoining => "開放加入",
        HostRecommendedAction.StartEvent => "開始活動",
        HostRecommendedAction.StartQuestion => "開始題目",
        HostRecommendedAction.CloseQuestion => "停止作答",
        HostRecommendedAction.RevealAnswer => "公布答案",
        HostRecommendedAction.ContinueQuiz => "進入下一題",
        HostRecommendedAction.ShowFinalLeaderboard => "顯示最終排行榜",
        HostRecommendedAction.CompleteEvent => "完成活動",
        _ => "查看已完成活動"
    };
}
