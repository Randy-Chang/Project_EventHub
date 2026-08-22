using EventHub.Host;
using System.Windows.Forms;

namespace EventHub.Host.Tests;

public sealed class HostDashboardLayoutTests
{
    [Fact]
    public void MainShell_UsesNavigationHeaderAndDedicatedViews()
    {
        using var form = new HostDashboardForm();

        var navigation = Assert.Single(form.Controls.Find("navigationPanel", true));
        var header = Assert.Single(form.Controls.Find("headerPanel", true));
        var content = Assert.Single(form.Controls.Find("contentPanel", true));
        var navigationButtons = new[]
        {
            "dashboardNavigationButton",
            "eventNavigationButton",
            "questionBankNavigationButton",
            "quizNavigationButton",
            "resultsNavigationButton",
            "displayNavigationButton"
        };
        var views = new[]
        {
            "dashboardView",
            "eventManagementView",
            "questionBankView",
            "quizControlView",
            "quizResultView",
            "displayControlView"
        };

        Assert.All(navigationButtons, name =>
            Assert.Equal(navigation, Assert.Single(form.Controls.Find(name, true)).Parent));
        Assert.All(views, name =>
        {
            var view = Assert.Single(form.Controls.Find(name, true));
            Assert.Equal(content, view.Parent);
            Assert.Equal(DockStyle.Fill, view.Dock);
        });
        Assert.True(header.Height > 0);
        Assert.True(form.MinimumSize.Width >= 1024);
        Assert.True(form.MinimumSize.Height >= 700);
    }

    [Fact]
    public void QuizControl_ExposesOnePrimaryWorkflowAction()
    {
        using var form = new HostDashboardForm();

        var quizView = Assert.Single(form.Controls.Find("quizControlView", true));
        var primaryAction = Assert.Single(quizView.Controls.Find("primaryActionButton", true));

        Assert.IsType<Button>(primaryAction);
        Assert.False(primaryAction.Enabled);
    }
}
