#nullable disable

namespace EventHub.Display;

partial class DisplayForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        setupPanel = new Panel();
        connectButton = new Button();
        fullscreenCheckBox = new CheckBox();
        screenComboBox = new ComboBox();
        screenLabel = new Label();
        eventIdTextBox = new TextBox();
        eventIdLabel = new Label();
        serverUrlTextBox = new TextBox();
        serverUrlLabel = new Label();
        connectionStatusLabel = new Label();
        presentationPanel = new Panel();
        waitingPanel = new TableLayoutPanel();
        eventTitleLabel = new Label();
        waitingContentTable = new TableLayoutPanel();
        qrCodePictureBox = new PictureBox();
        joinInfoTable = new TableLayoutPanel();
        waitingInstructionLabel = new Label();
        joinCodeTitleLabel = new Label();
        joinCodeLabel = new Label();
        joinUrlLabel = new Label();
        joinUrlWarningLabel = new Label();
        participantTable = new TableLayoutPanel();
        participantCountTitleLabel = new Label();
        participantCountLabel = new Label();
        questionPanel = new TableLayoutPanel();
        questionHeaderTable = new TableLayoutPanel();
        questionEventLabel = new Label();
        questionNumberLabel = new Label();
        questionTextLabel = new Label();
        questionOptionsTable = new TableLayoutPanel();
        optionALabel = new Label();
        optionBLabel = new Label();
        optionCLabel = new Label();
        optionDLabel = new Label();
        questionFooterTable = new TableLayoutPanel();
        questionStatusLabel = new Label();
        countdownLabel = new Label();
        questionProgressLabel = new Label();
        resultPanel = new TableLayoutPanel();
        resultEventLabel = new Label();
        correctAnswerLabel = new Label();
        correctRateLabel = new Label();
        distributionTable = new TableLayoutPanel();
        resultOptionALabel = new Label();
        resultOptionBLabel = new Label();
        resultOptionCLabel = new Label();
        resultOptionDLabel = new Label();
        resultOptionABar = new ProgressBar();
        resultOptionBBar = new ProgressBar();
        resultOptionCBar = new ProgressBar();
        resultOptionDBar = new ProgressBar();
        resultOptionACountLabel = new Label();
        resultOptionBCountLabel = new Label();
        resultOptionCCountLabel = new Label();
        resultOptionDCountLabel = new Label();
        leaderboardPanel = new TableLayoutPanel();
        leaderboardEventLabel = new Label();
        leaderboardTitleLabel = new Label();
        leaderboardGrid = new DataGridView();
        rankColumn = new DataGridViewTextBoxColumn();
        displayNameColumn = new DataGridViewTextBoxColumn();
        scoreColumn = new DataGridViewTextBoxColumn();
        setupPanel.SuspendLayout();
        presentationPanel.SuspendLayout();
        waitingPanel.SuspendLayout();
        waitingContentTable.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)qrCodePictureBox).BeginInit();
        joinInfoTable.SuspendLayout();
        participantTable.SuspendLayout();
        questionPanel.SuspendLayout();
        questionHeaderTable.SuspendLayout();
        questionOptionsTable.SuspendLayout();
        questionFooterTable.SuspendLayout();
        resultPanel.SuspendLayout();
        distributionTable.SuspendLayout();
        leaderboardPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)leaderboardGrid).BeginInit();
        SuspendLayout();
        //
        // setupPanel
        //
        setupPanel.BackColor = Color.FromArgb(15, 31, 51);
        setupPanel.Controls.Add(connectButton);
        setupPanel.Controls.Add(fullscreenCheckBox);
        setupPanel.Controls.Add(screenComboBox);
        setupPanel.Controls.Add(screenLabel);
        setupPanel.Controls.Add(eventIdTextBox);
        setupPanel.Controls.Add(eventIdLabel);
        setupPanel.Controls.Add(serverUrlTextBox);
        setupPanel.Controls.Add(serverUrlLabel);
        setupPanel.Dock = DockStyle.Top;
        setupPanel.Location = new Point(0, 0);
        setupPanel.Name = "setupPanel";
        setupPanel.Padding = new Padding(16, 12, 16, 10);
        setupPanel.Size = new Size(1280, 76);
        setupPanel.TabIndex = 0;
        //
        // connectButton
        //
        connectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        connectButton.BackColor = Color.FromArgb(24, 167, 112);
        connectButton.FlatAppearance.BorderSize = 0;
        connectButton.FlatStyle = FlatStyle.Flat;
        connectButton.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
        connectButton.ForeColor = Color.White;
        connectButton.Location = new Point(1156, 25);
        connectButton.Name = "connectButton";
        connectButton.Size = new Size(108, 36);
        connectButton.TabIndex = 7;
        connectButton.Text = "連線展示";
        connectButton.UseVisualStyleBackColor = false;
        connectButton.Click += connectButton_Click;
        //
        // fullscreenCheckBox
        //
        fullscreenCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        fullscreenCheckBox.AutoSize = true;
        fullscreenCheckBox.Font = new Font("Microsoft JhengHei UI", 10F);
        fullscreenCheckBox.ForeColor = Color.White;
        fullscreenCheckBox.Location = new Point(1064, 33);
        fullscreenCheckBox.Name = "fullscreenCheckBox";
        fullscreenCheckBox.Size = new Size(82, 22);
        fullscreenCheckBox.TabIndex = 6;
        fullscreenCheckBox.Text = "全螢幕";
        fullscreenCheckBox.UseVisualStyleBackColor = true;
        //
        // screenComboBox
        //
        screenComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        screenComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        screenComboBox.Font = new Font("Microsoft JhengHei UI", 10F);
        screenComboBox.FormattingEnabled = true;
        screenComboBox.Location = new Point(824, 32);
        screenComboBox.Name = "screenComboBox";
        screenComboBox.Size = new Size(225, 25);
        screenComboBox.TabIndex = 5;
        //
        // screenLabel
        //
        screenLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        screenLabel.AutoSize = true;
        screenLabel.Font = new Font("Microsoft JhengHei UI", 9F);
        screenLabel.ForeColor = Color.FromArgb(164, 184, 204);
        screenLabel.Location = new Point(824, 12);
        screenLabel.Name = "screenLabel";
        screenLabel.Size = new Size(56, 16);
        screenLabel.TabIndex = 4;
        screenLabel.Text = "顯示螢幕";
        //
        // eventIdTextBox
        //
        eventIdTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        eventIdTextBox.Font = new Font("Microsoft JhengHei UI", 10F);
        eventIdTextBox.Location = new Point(388, 32);
        eventIdTextBox.Name = "eventIdTextBox";
        eventIdTextBox.Size = new Size(420, 25);
        eventIdTextBox.TabIndex = 3;
        //
        // eventIdLabel
        //
        eventIdLabel.AutoSize = true;
        eventIdLabel.Font = new Font("Microsoft JhengHei UI", 9F);
        eventIdLabel.ForeColor = Color.FromArgb(164, 184, 204);
        eventIdLabel.Location = new Point(388, 12);
        eventIdLabel.Name = "eventIdLabel";
        eventIdLabel.Size = new Size(55, 16);
        eventIdLabel.TabIndex = 2;
        eventIdLabel.Text = "Event ID";
        //
        // serverUrlTextBox
        //
        serverUrlTextBox.Font = new Font("Microsoft JhengHei UI", 10F);
        serverUrlTextBox.Location = new Point(16, 32);
        serverUrlTextBox.Name = "serverUrlTextBox";
        serverUrlTextBox.Size = new Size(356, 25);
        serverUrlTextBox.TabIndex = 1;
        //
        // serverUrlLabel
        //
        serverUrlLabel.AutoSize = true;
        serverUrlLabel.Font = new Font("Microsoft JhengHei UI", 9F);
        serverUrlLabel.ForeColor = Color.FromArgb(164, 184, 204);
        serverUrlLabel.Location = new Point(16, 12);
        serverUrlLabel.Name = "serverUrlLabel";
        serverUrlLabel.Size = new Size(91, 16);
        serverUrlLabel.TabIndex = 0;
        serverUrlLabel.Text = "Server API URL";
        //
        // connectionStatusLabel
        //
        connectionStatusLabel.BackColor = Color.FromArgb(8, 20, 36);
        connectionStatusLabel.Dock = DockStyle.Top;
        connectionStatusLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
        connectionStatusLabel.ForeColor = Color.FromArgb(255, 190, 82);
        connectionStatusLabel.Location = new Point(0, 76);
        connectionStatusLabel.Name = "connectionStatusLabel";
        connectionStatusLabel.Padding = new Padding(16, 0, 16, 0);
        connectionStatusLabel.Size = new Size(1280, 30);
        connectionStatusLabel.TabIndex = 1;
        connectionStatusLabel.Text = "尚未連線 · F11 全螢幕 · Esc 離開全螢幕";
        connectionStatusLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // presentationPanel
        //
        presentationPanel.BackColor = Color.FromArgb(7, 20, 38);
        presentationPanel.Controls.Add(leaderboardPanel);
        presentationPanel.Controls.Add(resultPanel);
        presentationPanel.Controls.Add(questionPanel);
        presentationPanel.Controls.Add(waitingPanel);
        presentationPanel.Dock = DockStyle.Fill;
        presentationPanel.Location = new Point(0, 106);
        presentationPanel.Name = "presentationPanel";
        presentationPanel.Size = new Size(1280, 614);
        presentationPanel.TabIndex = 2;
        //
        // waitingPanel
        //
        waitingPanel.BackColor = Color.FromArgb(7, 20, 38);
        waitingPanel.ColumnCount = 1;
        waitingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        waitingPanel.Controls.Add(eventTitleLabel, 0, 0);
        waitingPanel.Controls.Add(waitingContentTable, 0, 1);
        waitingPanel.Controls.Add(participantTable, 0, 2);
        waitingPanel.Dock = DockStyle.Fill;
        waitingPanel.Location = new Point(0, 0);
        waitingPanel.Name = "waitingPanel";
        waitingPanel.Padding = new Padding(45, 20, 45, 20);
        waitingPanel.RowCount = 3;
        waitingPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 17F));
        waitingPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));
        waitingPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 19F));
        waitingPanel.Size = new Size(1280, 614);
        waitingPanel.TabIndex = 0;
        //
        // eventTitleLabel
        //
        eventTitleLabel.Dock = DockStyle.Fill;
        eventTitleLabel.Font = new Font("Microsoft JhengHei UI", 34F, FontStyle.Bold);
        eventTitleLabel.ForeColor = Color.White;
        eventTitleLabel.Location = new Point(48, 20);
        eventTitleLabel.Name = "eventTitleLabel";
        eventTitleLabel.Size = new Size(1184, 97);
        eventTitleLabel.TabIndex = 0;
        eventTitleLabel.Text = "EVENTHUB";
        eventTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // waitingContentTable
        //
        waitingContentTable.ColumnCount = 2;
        waitingContentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        waitingContentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        waitingContentTable.Controls.Add(qrCodePictureBox, 0, 0);
        waitingContentTable.Controls.Add(joinInfoTable, 1, 0);
        waitingContentTable.Dock = DockStyle.Fill;
        waitingContentTable.Location = new Point(48, 120);
        waitingContentTable.Name = "waitingContentTable";
        waitingContentTable.RowCount = 1;
        waitingContentTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        waitingContentTable.Size = new Size(1184, 361);
        waitingContentTable.TabIndex = 1;
        //
        // qrCodePictureBox
        //
        qrCodePictureBox.BackColor = Color.White;
        qrCodePictureBox.Dock = DockStyle.Fill;
        qrCodePictureBox.Location = new Point(50, 12);
        qrCodePictureBox.Margin = new Padding(50, 12, 50, 12);
        qrCodePictureBox.Name = "qrCodePictureBox";
        qrCodePictureBox.Size = new Size(586, 337);
        qrCodePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        qrCodePictureBox.TabIndex = 0;
        qrCodePictureBox.TabStop = false;
        //
        // joinInfoTable
        //
        joinInfoTable.ColumnCount = 1;
        joinInfoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        joinInfoTable.Controls.Add(waitingInstructionLabel, 0, 0);
        joinInfoTable.Controls.Add(joinCodeTitleLabel, 0, 1);
        joinInfoTable.Controls.Add(joinCodeLabel, 0, 2);
        joinInfoTable.Controls.Add(joinUrlLabel, 0, 3);
        joinInfoTable.Controls.Add(joinUrlWarningLabel, 0, 4);
        joinInfoTable.Dock = DockStyle.Fill;
        joinInfoTable.Location = new Point(689, 3);
        joinInfoTable.Name = "joinInfoTable";
        joinInfoTable.RowCount = 5;
        joinInfoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
        joinInfoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 14F));
        joinInfoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
        joinInfoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
        joinInfoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));
        joinInfoTable.Size = new Size(492, 355);
        joinInfoTable.TabIndex = 1;
        //
        // waitingInstructionLabel
        //
        waitingInstructionLabel.Dock = DockStyle.Fill;
        waitingInstructionLabel.Font = new Font("Microsoft JhengHei UI", 27F, FontStyle.Bold);
        waitingInstructionLabel.ForeColor = Color.FromArgb(94, 234, 170);
        waitingInstructionLabel.Location = new Point(3, 0);
        waitingInstructionLabel.Name = "waitingInstructionLabel";
        waitingInstructionLabel.Size = new Size(486, 99);
        waitingInstructionLabel.TabIndex = 0;
        waitingInstructionLabel.Text = "掃描 QR Code 加入活動";
        waitingInstructionLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // joinCodeTitleLabel
        //
        joinCodeTitleLabel.Dock = DockStyle.Fill;
        joinCodeTitleLabel.Font = new Font("Microsoft JhengHei UI", 15F);
        joinCodeTitleLabel.ForeColor = Color.FromArgb(164, 184, 204);
        joinCodeTitleLabel.Location = new Point(3, 99);
        joinCodeTitleLabel.Name = "joinCodeTitleLabel";
        joinCodeTitleLabel.Size = new Size(486, 49);
        joinCodeTitleLabel.TabIndex = 1;
        joinCodeTitleLabel.Text = "JOIN CODE";
        joinCodeTitleLabel.TextAlign = ContentAlignment.BottomCenter;
        //
        // joinCodeLabel
        //
        joinCodeLabel.Dock = DockStyle.Fill;
        joinCodeLabel.Font = new Font("Consolas", 46F, FontStyle.Bold);
        joinCodeLabel.ForeColor = Color.White;
        joinCodeLabel.Location = new Point(3, 148);
        joinCodeLabel.Name = "joinCodeLabel";
        joinCodeLabel.Size = new Size(486, 99);
        joinCodeLabel.TabIndex = 2;
        joinCodeLabel.Text = "------";
        joinCodeLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // joinUrlLabel
        //
        joinUrlLabel.Dock = DockStyle.Fill;
        joinUrlLabel.Font = new Font("Microsoft JhengHei UI", 13F);
        joinUrlLabel.ForeColor = Color.FromArgb(164, 184, 204);
        joinUrlLabel.Location = new Point(3, 247);
        joinUrlLabel.Name = "joinUrlLabel";
        joinUrlLabel.Size = new Size(486, 63);
        joinUrlLabel.TabIndex = 3;
        joinUrlLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // joinUrlWarningLabel
        //
        joinUrlWarningLabel.Dock = DockStyle.Fill;
        joinUrlWarningLabel.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
        joinUrlWarningLabel.ForeColor = Color.FromArgb(255, 130, 130);
        joinUrlWarningLabel.Location = new Point(3, 310);
        joinUrlWarningLabel.Name = "joinUrlWarningLabel";
        joinUrlWarningLabel.Size = new Size(486, 45);
        joinUrlWarningLabel.TabIndex = 4;
        joinUrlWarningLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // participantTable
        //
        participantTable.ColumnCount = 2;
        participantTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        participantTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        participantTable.Controls.Add(participantCountTitleLabel, 0, 0);
        participantTable.Controls.Add(participantCountLabel, 1, 0);
        participantTable.Dock = DockStyle.Fill;
        participantTable.Location = new Point(48, 484);
        participantTable.Name = "participantTable";
        participantTable.RowCount = 1;
        participantTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        participantTable.Size = new Size(1184, 107);
        participantTable.TabIndex = 2;
        //
        // participantCountTitleLabel
        //
        participantCountTitleLabel.Dock = DockStyle.Fill;
        participantCountTitleLabel.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold);
        participantCountTitleLabel.ForeColor = Color.FromArgb(164, 184, 204);
        participantCountTitleLabel.Location = new Point(3, 0);
        participantCountTitleLabel.Name = "participantCountTitleLabel";
        participantCountTitleLabel.Size = new Size(586, 107);
        participantCountTitleLabel.TabIndex = 0;
        participantCountTitleLabel.Text = "目前已加入";
        participantCountTitleLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // participantCountLabel
        //
        participantCountLabel.Dock = DockStyle.Fill;
        participantCountLabel.Font = new Font("Microsoft JhengHei UI", 48F, FontStyle.Bold);
        participantCountLabel.ForeColor = Color.FromArgb(94, 234, 170);
        participantCountLabel.Location = new Point(595, 0);
        participantCountLabel.Name = "participantCountLabel";
        participantCountLabel.Size = new Size(586, 107);
        participantCountLabel.TabIndex = 1;
        participantCountLabel.Text = "0";
        participantCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // questionPanel
        //
        questionPanel.ColumnCount = 1;
        questionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        questionPanel.Controls.Add(questionHeaderTable, 0, 0);
        questionPanel.Controls.Add(questionTextLabel, 0, 1);
        questionPanel.Controls.Add(questionOptionsTable, 0, 2);
        questionPanel.Controls.Add(questionFooterTable, 0, 3);
        questionPanel.Dock = DockStyle.Fill;
        questionPanel.Location = new Point(0, 0);
        questionPanel.Name = "questionPanel";
        questionPanel.Padding = new Padding(50, 25, 50, 25);
        questionPanel.RowCount = 4;
        questionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));
        questionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
        questionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 49F));
        questionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
        questionPanel.Size = new Size(1280, 614);
        questionPanel.TabIndex = 1;
        questionPanel.Visible = false;
        //
        // questionHeaderTable
        //
        questionHeaderTable.ColumnCount = 2;
        questionHeaderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        questionHeaderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        questionHeaderTable.Controls.Add(questionEventLabel, 0, 0);
        questionHeaderTable.Controls.Add(questionNumberLabel, 1, 0);
        questionHeaderTable.Dock = DockStyle.Fill;
        questionHeaderTable.Location = new Point(53, 28);
        questionHeaderTable.Name = "questionHeaderTable";
        questionHeaderTable.RowCount = 1;
        questionHeaderTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        questionHeaderTable.Size = new Size(1174, 61);
        questionHeaderTable.TabIndex = 0;
        //
        // questionEventLabel
        //
        questionEventLabel.Dock = DockStyle.Fill;
        questionEventLabel.Font = new Font("Microsoft JhengHei UI", 19F, FontStyle.Bold);
        questionEventLabel.ForeColor = Color.FromArgb(164, 184, 204);
        questionEventLabel.Location = new Point(3, 0);
        questionEventLabel.Name = "questionEventLabel";
        questionEventLabel.Size = new Size(815, 61);
        questionEventLabel.TabIndex = 0;
        questionEventLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // questionNumberLabel
        //
        questionNumberLabel.Dock = DockStyle.Fill;
        questionNumberLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        questionNumberLabel.ForeColor = Color.FromArgb(94, 234, 170);
        questionNumberLabel.Location = new Point(824, 0);
        questionNumberLabel.Name = "questionNumberLabel";
        questionNumberLabel.Size = new Size(347, 61);
        questionNumberLabel.TabIndex = 1;
        questionNumberLabel.Text = "第 1 / 1 題";
        questionNumberLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // questionTextLabel
        //
        questionTextLabel.Dock = DockStyle.Fill;
        questionTextLabel.Font = new Font("Microsoft JhengHei UI", 42F, FontStyle.Bold);
        questionTextLabel.ForeColor = Color.White;
        questionTextLabel.Location = new Point(53, 92);
        questionTextLabel.Name = "questionTextLabel";
        questionTextLabel.Padding = new Padding(15, 5, 15, 5);
        questionTextLabel.Size = new Size(1174, 129);
        questionTextLabel.TabIndex = 1;
        questionTextLabel.Text = "等待題目";
        questionTextLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // questionOptionsTable
        //
        questionOptionsTable.ColumnCount = 2;
        questionOptionsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        questionOptionsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        questionOptionsTable.Controls.Add(optionALabel, 0, 0);
        questionOptionsTable.Controls.Add(optionBLabel, 1, 0);
        questionOptionsTable.Controls.Add(optionCLabel, 0, 1);
        questionOptionsTable.Controls.Add(optionDLabel, 1, 1);
        questionOptionsTable.Dock = DockStyle.Fill;
        questionOptionsTable.Location = new Point(53, 224);
        questionOptionsTable.Name = "questionOptionsTable";
        questionOptionsTable.RowCount = 2;
        questionOptionsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        questionOptionsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        questionOptionsTable.Size = new Size(1174, 263);
        questionOptionsTable.TabIndex = 2;
        //
        // option labels
        //
        optionALabel.BackColor = Color.FromArgb(18, 43, 68);
        optionALabel.BorderStyle = BorderStyle.FixedSingle;
        optionALabel.Dock = DockStyle.Fill;
        optionALabel.Font = new Font("Microsoft JhengHei UI", 26F, FontStyle.Bold);
        optionALabel.ForeColor = Color.White;
        optionALabel.Margin = new Padding(8);
        optionALabel.Padding = new Padding(16);
        optionALabel.Text = "A.";
        optionALabel.TextAlign = ContentAlignment.MiddleLeft;
        optionBLabel.BackColor = Color.FromArgb(18, 43, 68);
        optionBLabel.BorderStyle = BorderStyle.FixedSingle;
        optionBLabel.Dock = DockStyle.Fill;
        optionBLabel.Font = new Font("Microsoft JhengHei UI", 26F, FontStyle.Bold);
        optionBLabel.ForeColor = Color.White;
        optionBLabel.Margin = new Padding(8);
        optionBLabel.Padding = new Padding(16);
        optionBLabel.Text = "B.";
        optionBLabel.TextAlign = ContentAlignment.MiddleLeft;
        optionCLabel.BackColor = Color.FromArgb(18, 43, 68);
        optionCLabel.BorderStyle = BorderStyle.FixedSingle;
        optionCLabel.Dock = DockStyle.Fill;
        optionCLabel.Font = new Font("Microsoft JhengHei UI", 26F, FontStyle.Bold);
        optionCLabel.ForeColor = Color.White;
        optionCLabel.Margin = new Padding(8);
        optionCLabel.Padding = new Padding(16);
        optionCLabel.Text = "C.";
        optionCLabel.TextAlign = ContentAlignment.MiddleLeft;
        optionDLabel.BackColor = Color.FromArgb(18, 43, 68);
        optionDLabel.BorderStyle = BorderStyle.FixedSingle;
        optionDLabel.Dock = DockStyle.Fill;
        optionDLabel.Font = new Font("Microsoft JhengHei UI", 26F, FontStyle.Bold);
        optionDLabel.ForeColor = Color.White;
        optionDLabel.Margin = new Padding(8);
        optionDLabel.Padding = new Padding(16);
        optionDLabel.Text = "D.";
        optionDLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // questionFooterTable
        //
        questionFooterTable.ColumnCount = 3;
        questionFooterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        questionFooterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
        questionFooterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        questionFooterTable.Controls.Add(questionStatusLabel, 0, 0);
        questionFooterTable.Controls.Add(countdownLabel, 1, 0);
        questionFooterTable.Controls.Add(questionProgressLabel, 2, 0);
        questionFooterTable.Dock = DockStyle.Fill;
        questionFooterTable.Location = new Point(53, 490);
        questionFooterTable.Name = "questionFooterTable";
        questionFooterTable.RowCount = 1;
        questionFooterTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        questionFooterTable.Size = new Size(1174, 96);
        questionFooterTable.TabIndex = 3;
        questionStatusLabel.Dock = DockStyle.Fill;
        questionStatusLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        questionStatusLabel.Name = "questionStatusLabel";
        questionStatusLabel.ForeColor = Color.FromArgb(94, 234, 170);
        questionStatusLabel.Text = "開放作答";
        questionStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        countdownLabel.Dock = DockStyle.Fill;
        countdownLabel.Font = new Font("Consolas", 45F, FontStyle.Bold);
        countdownLabel.Name = "countdownLabel";
        countdownLabel.ForeColor = Color.White;
        countdownLabel.Text = "00:00";
        countdownLabel.TextAlign = ContentAlignment.MiddleCenter;
        questionProgressLabel.Dock = DockStyle.Fill;
        questionProgressLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        questionProgressLabel.Name = "questionProgressLabel";
        questionProgressLabel.ForeColor = Color.FromArgb(164, 184, 204);
        questionProgressLabel.Text = "已作答 0 / 0";
        questionProgressLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // resultPanel
        //
        resultPanel.ColumnCount = 1;
        resultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        resultPanel.Controls.Add(resultEventLabel, 0, 0);
        resultPanel.Controls.Add(correctAnswerLabel, 0, 1);
        resultPanel.Controls.Add(correctRateLabel, 0, 2);
        resultPanel.Controls.Add(distributionTable, 0, 3);
        resultPanel.Dock = DockStyle.Fill;
        resultPanel.Location = new Point(0, 0);
        resultPanel.Name = "resultPanel";
        resultPanel.Padding = new Padding(70, 25, 70, 35);
        resultPanel.RowCount = 4;
        resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 11F));
        resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 14F));
        resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 53F));
        resultPanel.Size = new Size(1280, 614);
        resultPanel.TabIndex = 2;
        resultPanel.Visible = false;
        resultEventLabel.Dock = DockStyle.Fill;
        resultEventLabel.Font = new Font("Microsoft JhengHei UI", 18F, FontStyle.Bold);
        resultEventLabel.ForeColor = Color.FromArgb(164, 184, 204);
        resultEventLabel.Name = "resultEventLabel";
        resultEventLabel.TextAlign = ContentAlignment.MiddleCenter;
        correctAnswerLabel.Dock = DockStyle.Fill;
        correctAnswerLabel.Font = new Font("Microsoft JhengHei UI", 38F, FontStyle.Bold);
        correctAnswerLabel.ForeColor = Color.FromArgb(94, 234, 170);
        correctAnswerLabel.Name = "correctAnswerLabel";
        correctAnswerLabel.Text = "等待公布答案";
        correctAnswerLabel.TextAlign = ContentAlignment.MiddleCenter;
        correctRateLabel.Dock = DockStyle.Fill;
        correctRateLabel.Font = new Font("Microsoft JhengHei UI", 30F, FontStyle.Bold);
        correctRateLabel.ForeColor = Color.White;
        correctRateLabel.Name = "correctRateLabel";
        correctRateLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // distributionTable
        //
        distributionTable.ColumnCount = 3;
        distributionTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        distributionTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        distributionTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
        distributionTable.Dock = DockStyle.Fill;
        distributionTable.Location = new Point(73, 278);
        distributionTable.Name = "distributionTable";
        distributionTable.RowCount = 4;
        distributionTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        distributionTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        distributionTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        distributionTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        distributionTable.Size = new Size(1134, 298);
        distributionTable.TabIndex = 3;
        distributionTable.Controls.Add(resultOptionALabel, 0, 0);
        distributionTable.Controls.Add(resultOptionABar, 1, 0);
        distributionTable.Controls.Add(resultOptionACountLabel, 2, 0);
        distributionTable.Controls.Add(resultOptionBLabel, 0, 1);
        distributionTable.Controls.Add(resultOptionBBar, 1, 1);
        distributionTable.Controls.Add(resultOptionBCountLabel, 2, 1);
        distributionTable.Controls.Add(resultOptionCLabel, 0, 2);
        distributionTable.Controls.Add(resultOptionCBar, 1, 2);
        distributionTable.Controls.Add(resultOptionCCountLabel, 2, 2);
        distributionTable.Controls.Add(resultOptionDLabel, 0, 3);
        distributionTable.Controls.Add(resultOptionDBar, 1, 3);
        distributionTable.Controls.Add(resultOptionDCountLabel, 2, 3);
        resultOptionALabel.Dock = DockStyle.Fill;
        resultOptionALabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        resultOptionALabel.ForeColor = Color.White;
        resultOptionALabel.Name = "resultOptionALabel";
        resultOptionALabel.Text = "A.";
        resultOptionALabel.TextAlign = ContentAlignment.MiddleLeft;
        resultOptionBLabel.Dock = DockStyle.Fill;
        resultOptionBLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        resultOptionBLabel.ForeColor = Color.White;
        resultOptionBLabel.Name = "resultOptionBLabel";
        resultOptionBLabel.Text = "B.";
        resultOptionBLabel.TextAlign = ContentAlignment.MiddleLeft;
        resultOptionCLabel.Dock = DockStyle.Fill;
        resultOptionCLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        resultOptionCLabel.ForeColor = Color.White;
        resultOptionCLabel.Name = "resultOptionCLabel";
        resultOptionCLabel.Text = "C.";
        resultOptionCLabel.TextAlign = ContentAlignment.MiddleLeft;
        resultOptionDLabel.Dock = DockStyle.Fill;
        resultOptionDLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        resultOptionDLabel.ForeColor = Color.White;
        resultOptionDLabel.Name = "resultOptionDLabel";
        resultOptionDLabel.Text = "D.";
        resultOptionDLabel.TextAlign = ContentAlignment.MiddleLeft;
        resultOptionABar.Dock = DockStyle.Fill;
        resultOptionABar.Margin = new Padding(8, 22, 8, 22);
        resultOptionABar.Name = "resultOptionABar";
        resultOptionBBar.Dock = DockStyle.Fill;
        resultOptionBBar.Margin = new Padding(8, 22, 8, 22);
        resultOptionBBar.Name = "resultOptionBBar";
        resultOptionCBar.Dock = DockStyle.Fill;
        resultOptionCBar.Margin = new Padding(8, 22, 8, 22);
        resultOptionCBar.Name = "resultOptionCBar";
        resultOptionDBar.Dock = DockStyle.Fill;
        resultOptionDBar.Margin = new Padding(8, 22, 8, 22);
        resultOptionDBar.Name = "resultOptionDBar";
        resultOptionACountLabel.Dock = DockStyle.Fill;
        resultOptionACountLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        resultOptionACountLabel.ForeColor = Color.White;
        resultOptionACountLabel.Name = "resultOptionACountLabel";
        resultOptionACountLabel.Text = "0";
        resultOptionACountLabel.TextAlign = ContentAlignment.MiddleRight;
        resultOptionBCountLabel.Dock = DockStyle.Fill;
        resultOptionBCountLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        resultOptionBCountLabel.ForeColor = Color.White;
        resultOptionBCountLabel.Name = "resultOptionBCountLabel";
        resultOptionBCountLabel.Text = "0";
        resultOptionBCountLabel.TextAlign = ContentAlignment.MiddleRight;
        resultOptionCCountLabel.Dock = DockStyle.Fill;
        resultOptionCCountLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        resultOptionCCountLabel.ForeColor = Color.White;
        resultOptionCCountLabel.Name = "resultOptionCCountLabel";
        resultOptionCCountLabel.Text = "0";
        resultOptionCCountLabel.TextAlign = ContentAlignment.MiddleRight;
        resultOptionDCountLabel.Dock = DockStyle.Fill;
        resultOptionDCountLabel.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        resultOptionDCountLabel.ForeColor = Color.White;
        resultOptionDCountLabel.Name = "resultOptionDCountLabel";
        resultOptionDCountLabel.Text = "0";
        resultOptionDCountLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // leaderboardPanel
        //
        leaderboardPanel.ColumnCount = 1;
        leaderboardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leaderboardPanel.Controls.Add(leaderboardEventLabel, 0, 0);
        leaderboardPanel.Controls.Add(leaderboardTitleLabel, 0, 1);
        leaderboardPanel.Controls.Add(leaderboardGrid, 0, 2);
        leaderboardPanel.Dock = DockStyle.Fill;
        leaderboardPanel.Location = new Point(0, 0);
        leaderboardPanel.Name = "leaderboardPanel";
        leaderboardPanel.Padding = new Padding(100, 25, 100, 45);
        leaderboardPanel.RowCount = 3;
        leaderboardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
        leaderboardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
        leaderboardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 72F));
        leaderboardPanel.Size = new Size(1280, 614);
        leaderboardPanel.TabIndex = 3;
        leaderboardPanel.Visible = false;
        leaderboardEventLabel.Dock = DockStyle.Fill;
        leaderboardEventLabel.Font = new Font("Microsoft JhengHei UI", 18F, FontStyle.Bold);
        leaderboardEventLabel.Name = "leaderboardEventLabel";
        leaderboardEventLabel.ForeColor = Color.FromArgb(164, 184, 204);
        leaderboardEventLabel.TextAlign = ContentAlignment.MiddleCenter;
        leaderboardTitleLabel.Dock = DockStyle.Fill;
        leaderboardTitleLabel.Font = new Font("Microsoft JhengHei UI", 40F, FontStyle.Bold);
        leaderboardTitleLabel.Name = "leaderboardTitleLabel";
        leaderboardTitleLabel.ForeColor = Color.FromArgb(94, 234, 170);
        leaderboardTitleLabel.Text = "LEADERBOARD";
        leaderboardTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // leaderboardGrid
        //
        leaderboardGrid.AllowUserToAddRows = false;
        leaderboardGrid.AllowUserToDeleteRows = false;
        leaderboardGrid.AllowUserToResizeColumns = false;
        leaderboardGrid.AllowUserToResizeRows = false;
        leaderboardGrid.BackgroundColor = Color.FromArgb(7, 20, 38);
        leaderboardGrid.BorderStyle = BorderStyle.None;
        leaderboardGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        leaderboardGrid.ColumnHeadersVisible = false;
        leaderboardGrid.Columns.AddRange(new DataGridViewColumn[] { rankColumn, displayNameColumn, scoreColumn });
        leaderboardGrid.DefaultCellStyle.BackColor = Color.FromArgb(12, 32, 54);
        leaderboardGrid.DefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 22F, FontStyle.Bold);
        leaderboardGrid.DefaultCellStyle.ForeColor = Color.White;
        leaderboardGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(12, 32, 54);
        leaderboardGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        leaderboardGrid.Dock = DockStyle.Fill;
        leaderboardGrid.EnableHeadersVisualStyles = false;
        leaderboardGrid.GridColor = Color.FromArgb(40, 65, 88);
        leaderboardGrid.Location = new Point(103, 177);
        leaderboardGrid.MultiSelect = false;
        leaderboardGrid.Name = "leaderboardGrid";
        leaderboardGrid.ReadOnly = true;
        leaderboardGrid.RowHeadersVisible = false;
        leaderboardGrid.RowTemplate.Height = 42;
        leaderboardGrid.ScrollBars = ScrollBars.None;
        leaderboardGrid.Size = new Size(1074, 389);
        leaderboardGrid.TabIndex = 2;
        rankColumn.FillWeight = 15F;
        rankColumn.HeaderText = "Rank";
        rankColumn.Name = "rankColumn";
        rankColumn.ReadOnly = true;
        rankColumn.Width = 150;
        displayNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        displayNameColumn.HeaderText = "DisplayName";
        displayNameColumn.Name = "displayNameColumn";
        displayNameColumn.ReadOnly = true;
        scoreColumn.HeaderText = "Score";
        scoreColumn.Name = "scoreColumn";
        scoreColumn.ReadOnly = true;
        scoreColumn.Width = 260;
        //
        // DisplayForm
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(7, 20, 38);
        ClientSize = new Size(1152, 648);
        Controls.Add(presentationPanel);
        Controls.Add(connectionStatusLabel);
        Controls.Add(setupPanel);
        Font = new Font("Microsoft JhengHei UI", 9F);
        KeyPreview = true;
        MinimumSize = new Size(960, 540);
        Name = "DisplayForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EventHub Display";
        FormClosed += DisplayForm_FormClosed;
        Shown += DisplayForm_Shown;
        KeyDown += DisplayForm_KeyDown;
        setupPanel.ResumeLayout(false);
        setupPanel.PerformLayout();
        presentationPanel.ResumeLayout(false);
        waitingPanel.ResumeLayout(false);
        waitingContentTable.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)qrCodePictureBox).EndInit();
        joinInfoTable.ResumeLayout(false);
        participantTable.ResumeLayout(false);
        questionPanel.ResumeLayout(false);
        questionHeaderTable.ResumeLayout(false);
        questionOptionsTable.ResumeLayout(false);
        questionFooterTable.ResumeLayout(false);
        resultPanel.ResumeLayout(false);
        distributionTable.ResumeLayout(false);
        leaderboardPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)leaderboardGrid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel setupPanel;
    private Button connectButton;
    private CheckBox fullscreenCheckBox;
    private ComboBox screenComboBox;
    private Label screenLabel;
    private TextBox eventIdTextBox;
    private Label eventIdLabel;
    private TextBox serverUrlTextBox;
    private Label serverUrlLabel;
    private Label connectionStatusLabel;
    private Panel presentationPanel;
    private TableLayoutPanel waitingPanel;
    private Label eventTitleLabel;
    private TableLayoutPanel waitingContentTable;
    private PictureBox qrCodePictureBox;
    private TableLayoutPanel joinInfoTable;
    private Label waitingInstructionLabel;
    private Label joinCodeTitleLabel;
    private Label joinCodeLabel;
    private Label joinUrlLabel;
    private Label joinUrlWarningLabel;
    private TableLayoutPanel participantTable;
    private Label participantCountTitleLabel;
    private Label participantCountLabel;
    private TableLayoutPanel questionPanel;
    private TableLayoutPanel questionHeaderTable;
    private Label questionEventLabel;
    private Label questionNumberLabel;
    private Label questionTextLabel;
    private TableLayoutPanel questionOptionsTable;
    private Label optionALabel;
    private Label optionBLabel;
    private Label optionCLabel;
    private Label optionDLabel;
    private TableLayoutPanel questionFooterTable;
    private Label questionStatusLabel;
    private Label countdownLabel;
    private Label questionProgressLabel;
    private TableLayoutPanel resultPanel;
    private Label resultEventLabel;
    private Label correctAnswerLabel;
    private Label correctRateLabel;
    private TableLayoutPanel distributionTable;
    private Label resultOptionALabel;
    private Label resultOptionBLabel;
    private Label resultOptionCLabel;
    private Label resultOptionDLabel;
    private ProgressBar resultOptionABar;
    private ProgressBar resultOptionBBar;
    private ProgressBar resultOptionCBar;
    private ProgressBar resultOptionDBar;
    private Label resultOptionACountLabel;
    private Label resultOptionBCountLabel;
    private Label resultOptionCCountLabel;
    private Label resultOptionDCountLabel;
    private TableLayoutPanel leaderboardPanel;
    private Label leaderboardEventLabel;
    private Label leaderboardTitleLabel;
    private DataGridView leaderboardGrid;
    private DataGridViewTextBoxColumn rankColumn;
    private DataGridViewTextBoxColumn displayNameColumn;
    private DataGridViewTextBoxColumn scoreColumn;
}
