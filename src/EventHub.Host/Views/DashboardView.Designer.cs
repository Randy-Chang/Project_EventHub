#nullable disable

namespace EventHub.Host.Views;

partial class DashboardView
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        titleLabel = new Label();
        cardsTableLayoutPanel = new TableLayoutPanel();
        currentEventTitleLabel = new Label();
        currentEventValueLabel = new Label();
        currentQuizTitleLabel = new Label();
        currentQuizValueLabel = new Label();
        participantCountTitleLabel = new Label();
        participantCountValueLabel = new Label();
        questionTitleLabel = new Label();
        questionValueLabel = new Label();
        quizStateTitleLabel = new Label();
        quizStateValueLabel = new Label();
        serverTitleLabel = new Label();
        serverValueLabel = new Label();
        displayTitleLabel = new Label();
        displayValueLabel = new Label();
        cardsTableLayoutPanel.SuspendLayout();
        SuspendLayout();
        currentEventTitleLabel.Name = "currentEventTitleLabel";
        currentEventValueLabel.Name = "currentEventValueLabel";
        currentQuizTitleLabel.Name = "currentQuizTitleLabel";
        currentQuizValueLabel.Name = "currentQuizValueLabel";
        participantCountTitleLabel.Name = "participantCountTitleLabel";
        participantCountValueLabel.Name = "participantCountValueLabel";
        questionTitleLabel.Name = "questionTitleLabel";
        questionValueLabel.Name = "questionValueLabel";
        quizStateTitleLabel.Name = "quizStateTitleLabel";
        quizStateValueLabel.Name = "quizStateValueLabel";
        serverTitleLabel.Name = "serverTitleLabel";
        serverValueLabel.Name = "serverValueLabel";
        displayTitleLabel.Name = "displayTitleLabel";
        displayValueLabel.Name = "displayValueLabel";
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        titleLabel.Location = new Point(32, 28);
        titleLabel.Margin = new Padding(32, 28, 32, 16);
        titleLabel.Name = "titleLabel";
        titleLabel.Padding = new Padding(0, 12, 0, 0);
        titleLabel.Size = new Size(920, 60);
        titleLabel.Text = "活動控制台";
        cardsTableLayoutPanel.ColumnCount = 2;
        cardsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        cardsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        cardsTableLayoutPanel.Controls.Add(currentEventTitleLabel, 0, 0);
        cardsTableLayoutPanel.Controls.Add(currentEventValueLabel, 0, 1);
        cardsTableLayoutPanel.Controls.Add(currentQuizTitleLabel, 1, 0);
        cardsTableLayoutPanel.Controls.Add(currentQuizValueLabel, 1, 1);
        cardsTableLayoutPanel.Controls.Add(participantCountTitleLabel, 0, 2);
        cardsTableLayoutPanel.Controls.Add(participantCountValueLabel, 0, 3);
        cardsTableLayoutPanel.Controls.Add(questionTitleLabel, 1, 2);
        cardsTableLayoutPanel.Controls.Add(questionValueLabel, 1, 3);
        cardsTableLayoutPanel.Controls.Add(quizStateTitleLabel, 0, 4);
        cardsTableLayoutPanel.Controls.Add(quizStateValueLabel, 0, 5);
        cardsTableLayoutPanel.Controls.Add(serverTitleLabel, 1, 4);
        cardsTableLayoutPanel.Controls.Add(serverValueLabel, 1, 5);
        cardsTableLayoutPanel.Controls.Add(displayTitleLabel, 0, 6);
        cardsTableLayoutPanel.Controls.Add(displayValueLabel, 0, 7);
        cardsTableLayoutPanel.Dock = DockStyle.Fill;
        cardsTableLayoutPanel.Location = new Point(32, 100);
        cardsTableLayoutPanel.Margin = new Padding(20);
        cardsTableLayoutPanel.Name = "cardsTableLayoutPanel";
        cardsTableLayoutPanel.Padding = new Padding(16);
        cardsTableLayoutPanel.RowCount = 8;
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        cardsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        cardsTableLayoutPanel.Size = new Size(920, 600);
        ConfigureTitle(currentEventTitleLabel, "CURRENT EVENT");
        ConfigureValue(currentEventValueLabel, "尚未選擇活動");
        ConfigureTitle(currentQuizTitleLabel, "CURRENT QUIZ");
        ConfigureValue(currentQuizValueLabel, "尚未選擇題庫");
        ConfigureTitle(participantCountTitleLabel, "PARTICIPANTS");
        ConfigureValue(participantCountValueLabel, "0");
        ConfigureTitle(questionTitleLabel, "QUESTION");
        ConfigureValue(questionValueLabel, "— / —");
        ConfigureTitle(quizStateTitleLabel, "QUIZ STATE");
        ConfigureValue(quizStateValueLabel, "Waiting");
        ConfigureTitle(serverTitleLabel, "SERVER");
        ConfigureValue(serverValueLabel, "● Disconnected");
        ConfigureTitle(displayTitleLabel, "DISPLAY");
        ConfigureValue(displayValueLabel, "● Waiting");
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Controls.Add(cardsTableLayoutPanel);
        Controls.Add(titleLabel);
        Name = "DashboardView";
        Padding = new Padding(20);
        Size = new Size(984, 728);
        cardsTableLayoutPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void ConfigureTitle(Label label, string text)
    {
        label.Dock = DockStyle.Fill;
        label.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
        label.ForeColor = Color.DimGray;
        label.Text = text;
    }

    private static void ConfigureValue(Label label, string text)
    {
        label.AutoEllipsis = true;
        label.Dock = DockStyle.Fill;
        label.Font = new Font("Microsoft JhengHei UI", 18F, FontStyle.Bold);
        label.ForeColor = Color.FromArgb(24, 50, 79);
        label.Text = text;
    }

    private Label titleLabel;
    private TableLayoutPanel cardsTableLayoutPanel;
    private Label currentEventTitleLabel;
    private Label currentEventValueLabel;
    private Label currentQuizTitleLabel;
    private Label currentQuizValueLabel;
    private Label participantCountTitleLabel;
    private Label participantCountValueLabel;
    private Label questionTitleLabel;
    private Label questionValueLabel;
    private Label quizStateTitleLabel;
    private Label quizStateValueLabel;
    private Label serverTitleLabel;
    private Label serverValueLabel;
    private Label displayTitleLabel;
    private Label displayValueLabel;
}
