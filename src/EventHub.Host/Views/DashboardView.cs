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
        DisplayMode displayMode)
    {
        currentEventValueLabel.Text = eventName;
        currentQuizValueLabel.Text = quizTitle;
        participantCountValueLabel.Text = participantCount.ToString();
        questionValueLabel.Text = questionPosition;
        quizStateValueLabel.Text = quizState.ToString();
        serverValueLabel.Text = connectionState switch
        {
            HostConnectionState.Connected => "● Connected",
            HostConnectionState.Connecting => "● Connecting...",
            HostConnectionState.Reconnecting => "● Reconnecting...",
            _ => "● Disconnected"
        };
        displayValueLabel.Text = $"● {displayMode}";
    }
}
