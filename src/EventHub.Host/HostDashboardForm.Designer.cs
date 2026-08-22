#nullable disable

using EventHub.Host.Views;

namespace EventHub.Host;

partial class HostDashboardForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        shellTableLayoutPanel = new TableLayoutPanel();
        navigationPanel = new Panel();
        brandLabel = new Label();
        dashboardNavigationButton = new Button();
        eventNavigationButton = new Button();
        questionBankNavigationButton = new Button();
        quizNavigationButton = new Button();
        resultsNavigationButton = new Button();
        displayNavigationButton = new Button();
        headerPanel = new Panel();
        currentEventHeaderLabel = new Label();
        currentQuizHeaderLabel = new Label();
        participantHeaderLabel = new Label();
        serverHeaderLabel = new Label();
        displayHeaderLabel = new Label();
        contentPanel = new Panel();
        dashboardView = new DashboardView();
        eventManagementView = new EventManagementView();
        questionBankView = new QuestionBankView();
        quizControlView = new QuizControlView();
        quizResultView = new QuizResultView();
        displayControlView = new DisplayControlView();
        shellTableLayoutPanel.SuspendLayout();
        navigationPanel.SuspendLayout();
        headerPanel.SuspendLayout();
        contentPanel.SuspendLayout();
        SuspendLayout();
        shellTableLayoutPanel.Name = "shellTableLayoutPanel";
        navigationPanel.Name = "navigationPanel";
        brandLabel.Name = "brandLabel";
        dashboardNavigationButton.Name = "dashboardNavigationButton";
        eventNavigationButton.Name = "eventNavigationButton";
        questionBankNavigationButton.Name = "questionBankNavigationButton";
        quizNavigationButton.Name = "quizNavigationButton";
        resultsNavigationButton.Name = "resultsNavigationButton";
        displayNavigationButton.Name = "displayNavigationButton";
        headerPanel.Name = "headerPanel";
        currentEventHeaderLabel.Name = "currentEventHeaderLabel";
        currentQuizHeaderLabel.Name = "currentQuizHeaderLabel";
        participantHeaderLabel.Name = "participantHeaderLabel";
        serverHeaderLabel.Name = "serverHeaderLabel";
        displayHeaderLabel.Name = "displayHeaderLabel";
        contentPanel.Name = "contentPanel";
        dashboardView.Name = "dashboardView";
        eventManagementView.Name = "eventManagementView";
        questionBankView.Name = "questionBankView";
        quizControlView.Name = "quizControlView";
        quizResultView.Name = "quizResultView";
        displayControlView.Name = "displayControlView";
        shellTableLayoutPanel.ColumnCount = 2;
        shellTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        shellTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        shellTableLayoutPanel.Controls.Add(navigationPanel, 0, 0);
        shellTableLayoutPanel.SetRowSpan(navigationPanel, 2);
        shellTableLayoutPanel.Controls.Add(headerPanel, 1, 0);
        shellTableLayoutPanel.Controls.Add(contentPanel, 1, 1);
        shellTableLayoutPanel.Dock = DockStyle.Fill;
        shellTableLayoutPanel.RowCount = 2;
        shellTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        shellTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        navigationPanel.BackColor = Color.FromArgb(22, 43, 65);
        navigationPanel.Controls.Add(displayNavigationButton);
        navigationPanel.Controls.Add(resultsNavigationButton);
        navigationPanel.Controls.Add(quizNavigationButton);
        navigationPanel.Controls.Add(questionBankNavigationButton);
        navigationPanel.Controls.Add(eventNavigationButton);
        navigationPanel.Controls.Add(dashboardNavigationButton);
        navigationPanel.Controls.Add(brandLabel);
        navigationPanel.Dock = DockStyle.Fill;
        brandLabel.Dock = DockStyle.Top;
        brandLabel.Font = new Font("Microsoft JhengHei UI", 16F, FontStyle.Bold);
        brandLabel.ForeColor = Color.White;
        brandLabel.Height = 88;
        brandLabel.Padding = new Padding(18, 24, 0, 0);
        brandLabel.Text = "EventHub";
        ConfigureNavigationButton(dashboardNavigationButton, "Dashboard", 88, HostView.Dashboard);
        ConfigureNavigationButton(eventNavigationButton, "Event", 142, HostView.Event);
        ConfigureNavigationButton(questionBankNavigationButton, "Question Bank", 196, HostView.QuestionBank);
        ConfigureNavigationButton(quizNavigationButton, "Quiz Control", 250, HostView.Quiz);
        ConfigureNavigationButton(resultsNavigationButton, "Results", 304, HostView.Results);
        ConfigureNavigationButton(displayNavigationButton, "Display", 358, HostView.Display);
        headerPanel.BackColor = Color.White;
        headerPanel.Controls.Add(currentEventHeaderLabel);
        headerPanel.Controls.Add(currentQuizHeaderLabel);
        headerPanel.Controls.Add(participantHeaderLabel);
        headerPanel.Controls.Add(serverHeaderLabel);
        headerPanel.Controls.Add(displayHeaderLabel);
        headerPanel.Dock = DockStyle.Fill;
        currentEventHeaderLabel.AutoEllipsis = true;
        currentEventHeaderLabel.Font = new Font("Microsoft JhengHei UI", 14F, FontStyle.Bold);
        currentEventHeaderLabel.Location = new Point(24, 16);
        currentEventHeaderLabel.Size = new Size(430, 30);
        currentEventHeaderLabel.Text = "Current Event：尚未選擇";
        currentQuizHeaderLabel.AutoEllipsis = true;
        currentQuizHeaderLabel.Location = new Point(24, 50);
        currentQuizHeaderLabel.Size = new Size(430, 24);
        currentQuizHeaderLabel.Text = "Quiz：尚未選擇";
        participantHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        participantHeaderLabel.Location = new Point(486, 20);
        participantHeaderLabel.Size = new Size(125, 24);
        participantHeaderLabel.Text = "Participants 0";
        serverHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        serverHeaderLabel.ForeColor = Color.Firebrick;
        serverHeaderLabel.Location = new Point(620, 20);
        serverHeaderLabel.Size = new Size(150, 24);
        serverHeaderLabel.Text = "● Server Disconnected";
        displayHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        displayHeaderLabel.Location = new Point(780, 20);
        displayHeaderLabel.Size = new Size(130, 24);
        displayHeaderLabel.Text = "Display: Waiting";
        contentPanel.BackColor = Color.FromArgb(244, 247, 250);
        contentPanel.Controls.Add(displayControlView);
        contentPanel.Controls.Add(quizResultView);
        contentPanel.Controls.Add(quizControlView);
        contentPanel.Controls.Add(questionBankView);
        contentPanel.Controls.Add(eventManagementView);
        contentPanel.Controls.Add(dashboardView);
        contentPanel.Dock = DockStyle.Fill;
        dashboardView.Dock = DockStyle.Fill;
        eventManagementView.Dock = DockStyle.Fill;
        eventManagementView.Visible = false;
        questionBankView.Dock = DockStyle.Fill;
        questionBankView.Visible = false;
        quizControlView.Dock = DockStyle.Fill;
        quizControlView.Visible = false;
        quizResultView.Dock = DockStyle.Fill;
        quizResultView.Visible = false;
        displayControlView.Dock = DockStyle.Fill;
        displayControlView.Visible = false;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1180, 820);
        Controls.Add(shellTableLayoutPanel);
        MinimumSize = new Size(1024, 700);
        Name = "HostDashboardForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EventHub Host Console";
        FormClosed += HostDashboardForm_FormClosed;
        shellTableLayoutPanel.ResumeLayout(false);
        navigationPanel.ResumeLayout(false);
        headerPanel.ResumeLayout(false);
        contentPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void ConfigureNavigationButton(Button button, string text, int top, HostView view)
    {
        button.BackColor = Color.FromArgb(22, 43, 65);
        button.FlatAppearance.BorderSize = 0;
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
        button.ForeColor = Color.WhiteSmoke;
        button.Location = new Point(0, top);
        button.Padding = new Padding(18, 0, 0, 0);
        button.Size = new Size(190, 54);
        button.Tag = view;
        button.Text = text;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.UseVisualStyleBackColor = false;
        button.Click += navigationButton_Click;
    }

    private TableLayoutPanel shellTableLayoutPanel;
    private Panel navigationPanel;
    private Label brandLabel;
    private Button dashboardNavigationButton;
    private Button eventNavigationButton;
    private Button questionBankNavigationButton;
    private Button quizNavigationButton;
    private Button resultsNavigationButton;
    private Button displayNavigationButton;
    private Panel headerPanel;
    private Label currentEventHeaderLabel;
    private Label currentQuizHeaderLabel;
    private Label participantHeaderLabel;
    private Label serverHeaderLabel;
    private Label displayHeaderLabel;
    private Panel contentPanel;
    private DashboardView dashboardView;
    private EventManagementView eventManagementView;
    private QuestionBankView questionBankView;
    private QuizControlView quizControlView;
    private QuizResultView quizResultView;
    private DisplayControlView displayControlView;
}
