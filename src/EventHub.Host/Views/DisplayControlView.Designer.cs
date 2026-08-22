#nullable disable

namespace EventHub.Host.Views;

partial class DisplayControlView
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
        currentModeCaptionLabel = new Label();
        currentModeValueLabel = new Label();
        explanationLabel = new Label();
        modeFlowLayoutPanel = new FlowLayoutPanel();
        waitingButton = new Button();
        questionButton = new Button();
        resultButton = new Button();
        leaderboardButton = new Button();
        statusLabel = new Label();
        modeFlowLayoutPanel.SuspendLayout();
        SuspendLayout();
        titleLabel.Name = "titleLabel";
        currentModeCaptionLabel.Name = "currentModeCaptionLabel";
        currentModeValueLabel.Name = "currentModeValueLabel";
        explanationLabel.Name = "explanationLabel";
        modeFlowLayoutPanel.Name = "modeFlowLayoutPanel";
        waitingButton.Name = "waitingButton";
        questionButton.Name = "questionButton";
        resultButton.Name = "resultButton";
        leaderboardButton.Name = "leaderboardButton";
        statusLabel.Name = "statusLabel";
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        titleLabel.Height = 58;
        titleLabel.Text = "大螢幕控制";
        currentModeCaptionLabel.Dock = DockStyle.Top;
        currentModeCaptionLabel.Font = new Font("Microsoft JhengHei UI", 11F);
        currentModeCaptionLabel.Height = 32;
        currentModeCaptionLabel.Text = "CURRENT DISPLAY";
        currentModeValueLabel.Dock = DockStyle.Top;
        currentModeValueLabel.Font = new Font("Microsoft JhengHei UI", 28F, FontStyle.Bold);
        currentModeValueLabel.ForeColor = Color.FromArgb(24, 86, 138);
        currentModeValueLabel.Height = 78;
        currentModeValueLabel.Text = "Waiting";
        explanationLabel.Dock = DockStyle.Top;
        explanationLabel.Height = 48;
        explanationLabel.Text = "選擇全場投影幕目前要顯示的內容。此操作只改變展示模式，不會修改 Quiz 結果。";
        modeFlowLayoutPanel.Controls.Add(waitingButton);
        modeFlowLayoutPanel.Controls.Add(questionButton);
        modeFlowLayoutPanel.Controls.Add(resultButton);
        modeFlowLayoutPanel.Controls.Add(leaderboardButton);
        modeFlowLayoutPanel.Dock = DockStyle.Top;
        modeFlowLayoutPanel.Height = 82;
        waitingButton.Size = new Size(150, 56);
        waitingButton.Text = "等待／QR Code";
        waitingButton.Click += waitingButton_Click;
        questionButton.Size = new Size(150, 56);
        questionButton.Text = "題目";
        questionButton.Click += questionButton_Click;
        resultButton.Size = new Size(150, 56);
        resultButton.Text = "結果";
        resultButton.Click += resultButton_Click;
        leaderboardButton.Size = new Size(150, 56);
        leaderboardButton.Text = "排行榜";
        leaderboardButton.Click += leaderboardButton_Click;
        statusLabel.Dock = DockStyle.Top;
        statusLabel.Height = 36;
        statusLabel.Text = "等待操作";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Controls.Add(statusLabel);
        Controls.Add(modeFlowLayoutPanel);
        Controls.Add(explanationLabel);
        Controls.Add(currentModeValueLabel);
        Controls.Add(currentModeCaptionLabel);
        Controls.Add(titleLabel);
        Name = "DisplayControlView";
        Padding = new Padding(32);
        Size = new Size(984, 728);
        modeFlowLayoutPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label titleLabel;
    private Label currentModeCaptionLabel;
    private Label currentModeValueLabel;
    private Label explanationLabel;
    private FlowLayoutPanel modeFlowLayoutPanel;
    private Button waitingButton;
    private Button questionButton;
    private Button resultButton;
    private Button leaderboardButton;
    private Label statusLabel;
}
