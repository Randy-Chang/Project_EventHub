using EventHub.Host;
using System.Windows.Forms;

namespace EventHub.Host.Tests;

public sealed class HostDashboardLayoutTests
{
    [Fact]
    public void QuizControls_AreReachableAndSectionsDoNotOverlap()
    {
        using var form = new HostDashboardForm();

        var questionBank = Assert.Single(form.Controls.Find("questionBankGroupBox", true));
        var quiz = Assert.Single(form.Controls.Find("quizGroupBox", true));
        var start = Assert.Single(form.Controls.Find("startQuestionButton", true));
        var close = Assert.Single(form.Controls.Find("closeQuestionButton", true));
        var reveal = Assert.Single(form.Controls.Find("revealAnswerButton", true));

        Assert.True(form.AutoScroll);
        Assert.Equal(questionBank, start.Parent);
        Assert.Equal(questionBank, close.Parent);
        Assert.Equal(questionBank, reveal.Parent);
        Assert.True(quiz.Top >= questionBank.Bottom);
        Assert.Equal(AnchorStyles.None, quiz.Anchor & AnchorStyles.Bottom);
        Assert.True(start.Bottom <= questionBank.ClientSize.Height);
    }
}
