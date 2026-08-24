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
        Assert.Equal(new Size(1024, 640), form.MinimumSize);
    }

    [Fact]
    public void HostShell_UsesDpiResilientSizingAndDockedNavigation()
    {
        using var form = new HostDashboardForm();

        Assert.True(form.ClientSize.Width <= 1180);
        Assert.True(form.ClientSize.Height <= 680);
        Assert.All(
            new[]
            {
                "dashboardNavigationButton",
                "eventNavigationButton",
                "questionBankNavigationButton",
                "quizNavigationButton"
            },
            name => Assert.Equal(
                DockStyle.Top,
                Assert.Single(form.Controls.Find(name, true)).Dock));

        var eventView = Assert.IsAssignableFrom<ScrollableControl>(
            Assert.Single(form.Controls.Find("eventManagementView", true)));
        Assert.True(eventView.AutoScroll);

        var commandPanel = Assert.IsType<FlowLayoutPanel>(
            Assert.Single(form.Controls.Find("commandPanel", true)));
        Assert.True(commandPanel.WrapContents);
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
        Assert.Equal("完成準備", primaryAction.Text);
    }

    [Fact]
    public void EventView_ExposesSecondaryJoinPolicyToggle()
    {
        using var form = new HostDashboardForm();

        var toggle = Assert.IsType<Button>(Assert.Single(
            form.Controls.Find("toggleJoinPolicyButton", true)));

        Assert.False(toggle.Enabled);
        Assert.Equal("開放報到", toggle.Text);
        Assert.True(toggle.MinimumSize.Height >= 40);
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
    public void EventView_UsesSequentialStepsAndIndependentCopyActions()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));
        var root = Assert.IsType<TableLayoutPanel>(Assert.Single(eventView.Controls.Find("rootLayoutPanel", true)));

        var expectedRows = new[]
        {
            ("networkStatusPanel", 3),
            ("eventSetupPanel", 4),
            ("currentEventPanel", 5),
            ("mobileJoinPanel", 6),
            ("participantsPanel", 7)
        };
        foreach (var (name, row) in expectedRows)
        {
            var step = Assert.Single(eventView.Controls.Find(name, true));
            Assert.Equal(root, step.Parent);
            Assert.Equal(row, root.GetRow(step));
        }

        Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("copyEventIdButton", true)));
        Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("copyJoinCodeButton", true)));
        Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("copyJoinUrlButton", true)));
        Assert.NotEqual('\0', Assert.IsType<TextBox>(Assert.Single(eventView.Controls.Find("hostTokenTextBox", true))).PasswordChar);
        Assert.StartsWith("STEP 1", Assert.Single(eventView.Controls.Find("networkStatusTitleLabel", true)).Text);
        Assert.StartsWith("STEP 2", Assert.Single(eventView.Controls.Find("eventSetupTitleLabel", true)).Text);
        Assert.StartsWith("STEP 3", Assert.Single(eventView.Controls.Find("currentEventTitleLabel", true)).Text);
        Assert.StartsWith("STEP 4", Assert.Single(eventView.Controls.Find("mobileJoinTitleLabel", true)).Text);
        Assert.StartsWith("STEP 5", Assert.Single(eventView.Controls.Find("participantsTitleLabel", true)).Text);
    }

    [Fact]
    public void EventView_PresentsPrimaryActionAndParticipantEmptyState()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));

        Assert.Equal("建立新活動", Assert.IsType<Button>(Assert.Single(eventView.Controls.Find("createEventButton", true))).Text);
        Assert.Equal(PictureBoxSizeMode.Zoom, Assert.IsType<PictureBox>(Assert.Single(eventView.Controls.Find("joinQrCodePictureBox", true))).SizeMode);
        Assert.Equal("尚未有參與者加入", Assert.IsType<Label>(Assert.Single(eventView.Controls.Find("participantEmptyLabel", true))).Text);
    }

    [Fact]
    public void EventView_ButtonsReserveDpiSafeMinimumHeight()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));
        var buttonNames = new[]
        {
            "copyEventIdButton",
            "copyJoinCodeButton",
            "copyJoinUrlButton",
            "createEventButton",
            "connectButton",
            "refreshLanAddressesButton",
            "testConnectionButton",
            "installFirewallRuleButton"
        };

        foreach (var name in buttonNames)
        {
            var button = Assert.IsType<Button>(Assert.Single(eventView.Controls.Find(name, true)));
            Assert.True(button.MinimumSize.Height >= 36, $"{name} does not reserve enough vertical space.");
        }
    }

    [Fact]
    public void EventView_ContentDrivenStepsPropagatePreferredHeightToTheScrollWorkspace()
    {
        using var form = new HostDashboardForm();
        var eventView = Assert.Single(form.Controls.Find("eventManagementView", true));
        var root = Assert.IsType<TableLayoutPanel>(Assert.Single(eventView.Controls.Find("rootLayoutPanel", true)));
        var networkPanel = Assert.IsType<Panel>(Assert.Single(eventView.Controls.Find("networkStatusPanel", true)));
        var networkTable = Assert.IsType<TableLayoutPanel>(Assert.Single(eventView.Controls.Find("networkTableLayoutPanel", true)));

        Assert.True(((ScrollableControl)eventView).AutoScroll);
        Assert.All(root.RowStyles.Cast<RowStyle>().Take(7), style => Assert.Equal(SizeType.AutoSize, style.SizeType));
        Assert.Equal(SizeType.Absolute, root.RowStyles[7].SizeType);
        Assert.True(networkPanel.AutoSize);
        Assert.Equal(AutoSizeMode.GrowAndShrink, networkPanel.AutoSizeMode);
        Assert.True(networkTable.AutoSize);
        Assert.Equal(DockStyle.Top, networkTable.Dock);
        Assert.All(networkTable.RowStyles.Cast<RowStyle>(), style => Assert.Equal(SizeType.AutoSize, style.SizeType));
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
