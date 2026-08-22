namespace EventHub.Host.Views;

internal partial class QuizResultView : UserControl
{
    public QuizResultView()
    {
        InitializeComponent();
    }

    public void Render(QuizStatisticsView statistics, QuizLeaderboardView leaderboard, Guid? correctOptionId)
    {
        correctOptionValueLabel.Text = correctOptionId.HasValue
            ? statistics.OptionDistribution
                .Select((option, index) => new { option.OptionId, Text = $"{(char)('A' + index)}. {option.Text}" })
                .FirstOrDefault(option => option.OptionId == correctOptionId.Value)?.Text ?? "已公布"
            : "等待公布";
        answeredValueLabel.Text = statistics.AnsweredCount.ToString();
        correctValueLabel.Text = statistics.CorrectCount.ToString();
        incorrectValueLabel.Text = statistics.IncorrectCount.ToString();
        noAnswerValueLabel.Text = statistics.NoAnswerCount.ToString();
        correctRateValueLabel.Text = statistics.CorrectRate.ToString("P1");
        distributionGrid.Rows.Clear();
        foreach (var (option, index) in statistics.OptionDistribution.Select((option, index) => (option, index)))
        {
            distributionGrid.Rows.Add($"{(char)('A' + index)}. {option.Text}", option.AnswerCount);
        }

        leaderboardGrid.Rows.Clear();
        foreach (var entry in leaderboard.Entries)
        {
            leaderboardGrid.Rows.Add(
                entry.Rank,
                entry.DisplayName,
                entry.TotalScore,
                entry.CorrectCount,
                entry.AnsweredCount);
        }
    }

    public void Clear()
    {
        correctOptionValueLabel.Text = "等待公布";
        answeredValueLabel.Text = "0";
        correctValueLabel.Text = "0";
        incorrectValueLabel.Text = "0";
        noAnswerValueLabel.Text = "0";
        correctRateValueLabel.Text = "0%";
        distributionGrid.Rows.Clear();
        leaderboardGrid.Rows.Clear();
    }
}
