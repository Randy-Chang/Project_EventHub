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
        var header = Assert.Single(form.Controls.Find("headerTableLayoutPanel", true));
        var content = Assert.Single(form.Controls.Find("contentPanel", true));
        var navigationButtons = new[]
        {
            "dashboardNavigationButton",
            "eventNavigationButton",
            "questionBankNavigationButton",
            "quizNavigationButton"
        };
        var views = new[]
        {
            "dashboardView",
            "eventManagementView",
            "questionBankView",
            "quizActivityView"
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
        Assert.Single(form.Controls.Find("liveMonitorView", true));
        Assert.Single(form.Controls.Find("actionTableLayoutPanel", true));
        Assert.True(form.MinimumSize.Width >= 1024);
        Assert.True(form.MinimumSize.Height >= 700);
    }

    [Fact]
    public void HostShellAndViews_UseConsistentDpiScaling()
    {
        using var form = new HostDashboardForm();
        var scalableControls = new[]
        {
            (ContainerControl)form,
            Assert.IsAssignableFrom<ContainerControl>(Assert.Single(form.Controls.Find("dashboardView", true))),
            Assert.IsAssignableFrom<ContainerControl>(Assert.Single(form.Controls.Find("eventManagementView", true))),
            Assert.IsAssignableFrom<ContainerControl>(Assert.Single(form.Controls.Find("questionBankView", true))),
            Assert.IsAssignableFrom<ContainerControl>(Assert.Single(form.Controls.Find("quizActivityView", true))),
            Assert.IsAssignableFrom<ContainerControl>(Assert.Single(form.Controls.Find("liveMonitorView", true)))
        };

        Assert.All(scalableControls, control =>
        {
            Assert.Equal(AutoScaleMode.Dpi, control.AutoScaleMode);
            Assert.Equal(new SizeF(96F, 96F), control.AutoScaleDimensions);
        });
    }

    [Fact]
    public void ActivityShell_ExposesOnePersistentPrimaryWorkflowAction()
    {
        using var form = new HostDashboardForm();

        var primaryAction = Assert.Single(form.Controls.Find("primaryActionButton", true));

        Assert.IsType<Button>(primaryAction);
        Assert.False(primaryAction.Enabled);
        Assert.Equal("請先選擇題庫", primaryAction.Text);
    }

    [Fact]
    public void EmptyQuizState_DoesNotLookLikeAnActiveOfficialQuestion()
    {
        using var form = new HostDashboardForm();

        Assert.Equal("Quiz：尚未選擇", Assert.Single(form.Controls.Find("currentQuizHeaderLabel", true)).Text);
        Assert.Equal("Activity: —", Assert.Single(form.Controls.Find("currentActivityHeaderLabel", true)).Text);
        Assert.Equal("NO QUIZ", Assert.Single(form.Controls.Find("activityStateHeaderLabel", true)).Text);
        var quizView = Assert.Single(form.Controls.Find("quizActivityView", true));
        Assert.Equal("—", Assert.Single(quizView.Controls.Find("modeValueLabel", true)).Text);
        Assert.Contains("NO QUIZ", form.Controls.Find("stateValueLabel", true).Select(control => control.Text));
    }

    [Fact]
    public void EventView_UsesDedicatedOperationMessagePanelBeforeJoinInformation()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));
        var messagePanel = Assert.Single(eventView.Controls.Find("operationMessagePanel", true));
        var message = Assert.Single(eventView.Controls.Find("statusLabel", true));

        Assert.Equal(messagePanel, message.Parent);
        Assert.Equal(DockStyle.Top, messagePanel.Dock);
        Assert.False(((Label)message).AutoEllipsis);
    }

    [Fact]
    public void EventView_ContainsDesignerCreatedNetworkDeploymentControls()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));

        Assert.IsType<ComboBox>(Assert.Single(eventView.Controls.Find("lanAddressComboBox", true)));
        Assert.Equal(
            "連線檢查",
            Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("testConnectionButton", true))).Text);
        Assert.Equal(
            "建立 Firewall Rule",
            Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("installFirewallRuleButton", true))).Text);
        Assert.Equal(
            "http://localhost:5000",
            Assert.IsType<TextBox>(Assert.Single(eventView.Controls.Find("serverUrlTextBox", true))).Text);
    }

    [Fact]
    public void QuizView_ContainsDesignerCreatedDefaultPracticeEntry()
    {
        using var form = new HostDashboardForm();
        var button = Assert.IsType<Button>(Assert.Single(form.Controls.Find("defaultPracticeButton", true)));

        Assert.Equal("執行內建熱身", button.Text);
    }
}
