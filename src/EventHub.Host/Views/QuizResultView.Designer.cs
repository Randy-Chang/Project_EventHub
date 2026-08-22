#nullable disable

namespace EventHub.Host.Views;

partial class QuizResultView
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
        summaryTableLayoutPanel = new TableLayoutPanel();
        correctOptionCaptionLabel = new Label();
        correctOptionValueLabel = new Label();
        answeredCaptionLabel = new Label();
        answeredValueLabel = new Label();
        correctCaptionLabel = new Label();
        correctValueLabel = new Label();
        incorrectCaptionLabel = new Label();
        incorrectValueLabel = new Label();
        noAnswerCaptionLabel = new Label();
        noAnswerValueLabel = new Label();
        correctRateCaptionLabel = new Label();
        correctRateValueLabel = new Label();
        splitContainer = new SplitContainer();
        distributionGroupBox = new GroupBox();
        distributionGrid = new DataGridView();
        leaderboardGroupBox = new GroupBox();
        leaderboardGrid = new DataGridView();
        summaryTableLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        distributionGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)distributionGrid).BeginInit();
        leaderboardGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)leaderboardGrid).BeginInit();
        SuspendLayout();
        titleLabel.Name = "titleLabel";
        summaryTableLayoutPanel.Name = "summaryTableLayoutPanel";
        correctOptionCaptionLabel.Name = "correctOptionCaptionLabel";
        correctOptionValueLabel.Name = "correctOptionValueLabel";
        answeredCaptionLabel.Name = "answeredCaptionLabel";
        answeredValueLabel.Name = "answeredValueLabel";
        correctCaptionLabel.Name = "correctCaptionLabel";
        correctValueLabel.Name = "correctValueLabel";
        incorrectCaptionLabel.Name = "incorrectCaptionLabel";
        incorrectValueLabel.Name = "incorrectValueLabel";
        noAnswerCaptionLabel.Name = "noAnswerCaptionLabel";
        noAnswerValueLabel.Name = "noAnswerValueLabel";
        correctRateCaptionLabel.Name = "correctRateCaptionLabel";
        correctRateValueLabel.Name = "correctRateValueLabel";
        splitContainer.Name = "splitContainer";
        distributionGroupBox.Name = "distributionGroupBox";
        distributionGrid.Name = "distributionGrid";
        leaderboardGroupBox.Name = "leaderboardGroupBox";
        leaderboardGrid.Name = "leaderboardGrid";
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        titleLabel.Height = 58;
        titleLabel.Text = "結果與排行榜";
        summaryTableLayoutPanel.ColumnCount = 6;
        for (var index = 0; index < 6; index++) summaryTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        summaryTableLayoutPanel.Controls.Add(correctOptionCaptionLabel, 0, 0);
        summaryTableLayoutPanel.Controls.Add(answeredCaptionLabel, 1, 0);
        summaryTableLayoutPanel.Controls.Add(correctCaptionLabel, 2, 0);
        summaryTableLayoutPanel.Controls.Add(incorrectCaptionLabel, 3, 0);
        summaryTableLayoutPanel.Controls.Add(noAnswerCaptionLabel, 4, 0);
        summaryTableLayoutPanel.Controls.Add(correctRateCaptionLabel, 5, 0);
        summaryTableLayoutPanel.Controls.Add(correctOptionValueLabel, 0, 1);
        summaryTableLayoutPanel.Controls.Add(answeredValueLabel, 1, 1);
        summaryTableLayoutPanel.Controls.Add(correctValueLabel, 2, 1);
        summaryTableLayoutPanel.Controls.Add(incorrectValueLabel, 3, 1);
        summaryTableLayoutPanel.Controls.Add(noAnswerValueLabel, 4, 1);
        summaryTableLayoutPanel.Controls.Add(correctRateValueLabel, 5, 1);
        summaryTableLayoutPanel.Dock = DockStyle.Top;
        summaryTableLayoutPanel.Height = 100;
        summaryTableLayoutPanel.RowCount = 2;
        summaryTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        summaryTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ConfigureCaption(correctOptionCaptionLabel, "正確答案");
        ConfigureCaption(answeredCaptionLabel, "作答");
        ConfigureCaption(correctCaptionLabel, "答對");
        ConfigureCaption(incorrectCaptionLabel, "答錯");
        ConfigureCaption(noAnswerCaptionLabel, "未作答");
        ConfigureCaption(correctRateCaptionLabel, "正確率");
        ConfigureValue(correctOptionValueLabel, "等待公布");
        ConfigureValue(answeredValueLabel, "0");
        ConfigureValue(correctValueLabel, "0");
        ConfigureValue(incorrectValueLabel, "0");
        ConfigureValue(noAnswerValueLabel, "0");
        ConfigureValue(correctRateValueLabel, "0%");
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Orientation = Orientation.Horizontal;
        splitContainer.Panel1.Controls.Add(distributionGroupBox);
        splitContainer.Panel2.Controls.Add(leaderboardGroupBox);
        splitContainer.SplitterDistance = 210;
        distributionGroupBox.Controls.Add(distributionGrid);
        distributionGroupBox.Dock = DockStyle.Fill;
        distributionGroupBox.Text = "選項分布";
        distributionGrid.AllowUserToAddRows = false;
        distributionGrid.AllowUserToDeleteRows = false;
        distributionGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        distributionGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "選項", FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "人數", FillWeight = 20 });
        distributionGrid.Dock = DockStyle.Fill;
        distributionGrid.ReadOnly = true;
        distributionGrid.RowHeadersVisible = false;
        leaderboardGroupBox.Controls.Add(leaderboardGrid);
        leaderboardGroupBox.Dock = DockStyle.Fill;
        leaderboardGroupBox.Text = "Leaderboard Top 10";
        leaderboardGrid.AllowUserToAddRows = false;
        leaderboardGrid.AllowUserToDeleteRows = false;
        leaderboardGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        leaderboardGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "名次" },
            new DataGridViewTextBoxColumn { HeaderText = "顯示名稱", FillWeight = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "總分" },
            new DataGridViewTextBoxColumn { HeaderText = "答對" },
            new DataGridViewTextBoxColumn { HeaderText = "作答" });
        leaderboardGrid.Dock = DockStyle.Fill;
        leaderboardGrid.ReadOnly = true;
        leaderboardGrid.RowHeadersVisible = false;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Controls.Add(splitContainer);
        Controls.Add(summaryTableLayoutPanel);
        Controls.Add(titleLabel);
        Name = "QuizResultView";
        Padding = new Padding(24);
        Size = new Size(984, 728);
        summaryTableLayoutPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        distributionGroupBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)distributionGrid).EndInit();
        leaderboardGroupBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)leaderboardGrid).EndInit();
        ResumeLayout(false);
    }

    private static void ConfigureCaption(Label label, string text)
    {
        label.Dock = DockStyle.Fill;
        label.ForeColor = Color.DimGray;
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleCenter;
    }

    private static void ConfigureValue(Label label, string text)
    {
        label.AutoEllipsis = true;
        label.Dock = DockStyle.Fill;
        label.Font = new Font("Microsoft JhengHei UI", 14F, FontStyle.Bold);
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleCenter;
    }

    private Label titleLabel;
    private TableLayoutPanel summaryTableLayoutPanel;
    private Label correctOptionCaptionLabel;
    private Label correctOptionValueLabel;
    private Label answeredCaptionLabel;
    private Label answeredValueLabel;
    private Label correctCaptionLabel;
    private Label correctValueLabel;
    private Label incorrectCaptionLabel;
    private Label incorrectValueLabel;
    private Label noAnswerCaptionLabel;
    private Label noAnswerValueLabel;
    private Label correctRateCaptionLabel;
    private Label correctRateValueLabel;
    private SplitContainer splitContainer;
    private GroupBox distributionGroupBox;
    private DataGridView distributionGrid;
    private GroupBox leaderboardGroupBox;
    private DataGridView leaderboardGrid;
}
