#nullable disable

namespace EventHub.Host;

partial class HostDashboardForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        serverUrlLabel = new Label();
        serverUrlTextBox = new TextBox();
        eventNameLabel = new Label();
        eventNameTextBox = new TextBox();
        eventDateLabel = new Label();
        eventDatePicker = new DateTimePicker();
        createEventButton = new Button();
        eventIdLabel = new Label();
        eventIdTextBox = new TextBox();
        hostTokenLabel = new Label();
        hostTokenTextBox = new TextBox();
        connectButton = new Button();
        statusLabel = new Label();
        onlineCountLabel = new Label();
        participantGrid = new DataGridView();
        nameColumn = new DataGridViewTextBoxColumn();
        employeeNumberColumn = new DataGridViewTextBoxColumn();
        departmentColumn = new DataGridViewTextBoxColumn();
        tableNumberColumn = new DataGridViewTextBoxColumn();
        onlineColumn = new DataGridViewTextBoxColumn();
        scoreColumn = new DataGridViewTextBoxColumn();
        quizGroupBox = new GroupBox();
        questionTextLabel = new Label();
        questionTextBox = new TextBox();
        optionALabel = new Label();
        optionATextBox = new TextBox();
        optionBLabel = new Label();
        optionBTextBox = new TextBox();
        optionCLabel = new Label();
        optionCTextBox = new TextBox();
        optionDLabel = new Label();
        optionDTextBox = new TextBox();
        correctOptionLabel = new Label();
        correctOptionComboBox = new ComboBox();
        answerDurationLabel = new Label();
        answerDurationNumeric = new NumericUpDown();
        createQuestionButton = new Button();
        currentQuestionLabel = new Label();
        quizStateLabel = new Label();
        quizProgressLabel = new Label();
        startQuestionButton = new Button();
        closeQuestionButton = new Button();
        revealAnswerButton = new Button();
        ((System.ComponentModel.ISupportInitialize)participantGrid).BeginInit();
        quizGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)answerDurationNumeric).BeginInit();
        SuspendLayout();
        //
        // serverUrlLabel
        //
        serverUrlLabel.AutoSize = true;
        serverUrlLabel.Location = new Point(20, 23);
        serverUrlLabel.Name = "serverUrlLabel";
        serverUrlLabel.Size = new Size(70, 15);
        serverUrlLabel.TabIndex = 0;
        serverUrlLabel.Text = "Server URL";
        //
        // serverUrlTextBox
        //
        serverUrlTextBox.Location = new Point(105, 20);
        serverUrlTextBox.Name = "serverUrlTextBox";
        serverUrlTextBox.Size = new Size(245, 23);
        serverUrlTextBox.TabIndex = 1;
        serverUrlTextBox.Text = "http://localhost:5000";
        //
        // eventNameLabel
        //
        eventNameLabel.AutoSize = true;
        eventNameLabel.Location = new Point(20, 61);
        eventNameLabel.Name = "eventNameLabel";
        eventNameLabel.Size = new Size(55, 15);
        eventNameLabel.TabIndex = 2;
        eventNameLabel.Text = "活動名稱";
        //
        // eventNameTextBox
        //
        eventNameTextBox.Location = new Point(105, 58);
        eventNameTextBox.Name = "eventNameTextBox";
        eventNameTextBox.Size = new Size(245, 23);
        eventNameTextBox.TabIndex = 3;
        eventNameTextBox.Text = "公司活動";
        //
        // eventDateLabel
        //
        eventDateLabel.AutoSize = true;
        eventDateLabel.Location = new Point(370, 61);
        eventDateLabel.Name = "eventDateLabel";
        eventDateLabel.Size = new Size(55, 15);
        eventDateLabel.TabIndex = 4;
        eventDateLabel.Text = "活動日期";
        //
        // eventDatePicker
        //
        eventDatePicker.CustomFormat = "yyyy/MM/dd HH:mm";
        eventDatePicker.Format = DateTimePickerFormat.Custom;
        eventDatePicker.Location = new Point(440, 58);
        eventDatePicker.Name = "eventDatePicker";
        eventDatePicker.Size = new Size(160, 23);
        eventDatePicker.TabIndex = 5;
        //
        // createEventButton
        //
        createEventButton.Location = new Point(620, 57);
        createEventButton.Name = "createEventButton";
        createEventButton.Size = new Size(105, 25);
        createEventButton.TabIndex = 6;
        createEventButton.Text = "建立活動";
        createEventButton.UseVisualStyleBackColor = true;
        createEventButton.Click += createEventButton_Click;
        //
        // eventIdLabel
        //
        eventIdLabel.AutoSize = true;
        eventIdLabel.Location = new Point(20, 101);
        eventIdLabel.Name = "eventIdLabel";
        eventIdLabel.Size = new Size(46, 15);
        eventIdLabel.TabIndex = 7;
        eventIdLabel.Text = "活動 ID";
        //
        // eventIdTextBox
        //
        eventIdTextBox.Location = new Point(105, 98);
        eventIdTextBox.Name = "eventIdTextBox";
        eventIdTextBox.Size = new Size(300, 23);
        eventIdTextBox.TabIndex = 8;
        //
        // hostTokenLabel
        //
        hostTokenLabel.AutoSize = true;
        hostTokenLabel.Location = new Point(420, 101);
        hostTokenLabel.Name = "hostTokenLabel";
        hostTokenLabel.Size = new Size(67, 15);
        hostTokenLabel.TabIndex = 9;
        hostTokenLabel.Text = "Host Token";
        //
        // hostTokenTextBox
        //
        hostTokenTextBox.Location = new Point(500, 98);
        hostTokenTextBox.Name = "hostTokenTextBox";
        hostTokenTextBox.PasswordChar = '●';
        hostTokenTextBox.Size = new Size(225, 23);
        hostTokenTextBox.TabIndex = 10;
        //
        // connectButton
        //
        connectButton.Location = new Point(740, 97);
        connectButton.Name = "connectButton";
        connectButton.Size = new Size(105, 25);
        connectButton.TabIndex = 11;
        connectButton.Text = "連線監看";
        connectButton.UseVisualStyleBackColor = true;
        connectButton.Click += connectButton_Click;
        //
        // statusLabel
        //
        statusLabel.AutoSize = true;
        statusLabel.Location = new Point(20, 143);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(79, 15);
        statusLabel.TabIndex = 12;
        statusLabel.Text = "尚未連線";
        //
        // onlineCountLabel
        //
        onlineCountLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        onlineCountLabel.Location = new Point(690, 143);
        onlineCountLabel.Name = "onlineCountLabel";
        onlineCountLabel.Size = new Size(155, 15);
        onlineCountLabel.TabIndex = 13;
        onlineCountLabel.Text = "在線 0 / 總計 0";
        onlineCountLabel.TextAlign = ContentAlignment.TopRight;
        //
        // participantGrid
        //
        participantGrid.AllowUserToAddRows = false;
        participantGrid.AllowUserToDeleteRows = false;
        participantGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        participantGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        participantGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        participantGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, employeeNumberColumn, departmentColumn, tableNumberColumn, onlineColumn, scoreColumn });
        participantGrid.Location = new Point(20, 170);
        participantGrid.MultiSelect = false;
        participantGrid.Name = "participantGrid";
        participantGrid.ReadOnly = true;
        participantGrid.RowHeadersVisible = false;
        participantGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        participantGrid.Size = new Size(825, 245);
        participantGrid.TabIndex = 14;
        //
        // columns
        //
        nameColumn.HeaderText = "顯示名稱";
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        employeeNumberColumn.HeaderText = "員工編號";
        employeeNumberColumn.Name = "employeeNumberColumn";
        employeeNumberColumn.ReadOnly = true;
        departmentColumn.HeaderText = "部門";
        departmentColumn.Name = "departmentColumn";
        departmentColumn.ReadOnly = true;
        tableNumberColumn.HeaderText = "桌次";
        tableNumberColumn.Name = "tableNumberColumn";
        tableNumberColumn.ReadOnly = true;
        onlineColumn.HeaderText = "狀態";
        onlineColumn.Name = "onlineColumn";
        onlineColumn.ReadOnly = true;
        scoreColumn.HeaderText = "分數";
        scoreColumn.Name = "scoreColumn";
        scoreColumn.ReadOnly = true;
        //
        // quizGroupBox
        //
        quizGroupBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        quizGroupBox.Controls.Add(questionTextLabel);
        quizGroupBox.Controls.Add(questionTextBox);
        quizGroupBox.Controls.Add(optionALabel);
        quizGroupBox.Controls.Add(optionATextBox);
        quizGroupBox.Controls.Add(optionBLabel);
        quizGroupBox.Controls.Add(optionBTextBox);
        quizGroupBox.Controls.Add(optionCLabel);
        quizGroupBox.Controls.Add(optionCTextBox);
        quizGroupBox.Controls.Add(optionDLabel);
        quizGroupBox.Controls.Add(optionDTextBox);
        quizGroupBox.Controls.Add(correctOptionLabel);
        quizGroupBox.Controls.Add(correctOptionComboBox);
        quizGroupBox.Controls.Add(answerDurationLabel);
        quizGroupBox.Controls.Add(answerDurationNumeric);
        quizGroupBox.Controls.Add(createQuestionButton);
        quizGroupBox.Controls.Add(currentQuestionLabel);
        quizGroupBox.Controls.Add(quizStateLabel);
        quizGroupBox.Controls.Add(quizProgressLabel);
        quizGroupBox.Controls.Add(startQuestionButton);
        quizGroupBox.Controls.Add(closeQuestionButton);
        quizGroupBox.Controls.Add(revealAnswerButton);
        quizGroupBox.Location = new Point(20, 430);
        quizGroupBox.Name = "quizGroupBox";
        quizGroupBox.Size = new Size(825, 285);
        quizGroupBox.TabIndex = 15;
        quizGroupBox.TabStop = false;
        quizGroupBox.Text = "Quiz 快問快答";
        //
        // questionTextLabel / questionTextBox
        //
        questionTextLabel.AutoSize = true;
        questionTextLabel.Location = new Point(15, 29);
        questionTextLabel.Text = "題目";
        questionTextBox.Location = new Point(55, 25);
        questionTextBox.Name = "questionTextBox";
        questionTextBox.Size = new Size(350, 23);
        questionTextBox.Text = "1 + 1 = ?";
        //
        // options
        //
        optionALabel.AutoSize = true;
        optionALabel.Location = new Point(15, 66);
        optionALabel.Text = "A";
        optionATextBox.Location = new Point(35, 62);
        optionATextBox.Name = "optionATextBox";
        optionATextBox.Size = new Size(170, 23);
        optionATextBox.Text = "1";
        optionBLabel.AutoSize = true;
        optionBLabel.Location = new Point(220, 66);
        optionBLabel.Text = "B";
        optionBTextBox.Location = new Point(240, 62);
        optionBTextBox.Name = "optionBTextBox";
        optionBTextBox.Size = new Size(165, 23);
        optionBTextBox.Text = "2";
        optionCLabel.AutoSize = true;
        optionCLabel.Location = new Point(15, 101);
        optionCLabel.Text = "C";
        optionCTextBox.Location = new Point(35, 97);
        optionCTextBox.Name = "optionCTextBox";
        optionCTextBox.Size = new Size(170, 23);
        optionCTextBox.Text = "3";
        optionDLabel.AutoSize = true;
        optionDLabel.Location = new Point(220, 101);
        optionDLabel.Text = "D";
        optionDTextBox.Location = new Point(240, 97);
        optionDTextBox.Name = "optionDTextBox";
        optionDTextBox.Size = new Size(165, 23);
        optionDTextBox.Text = "4";
        //
        // quiz authoring settings
        //
        correctOptionLabel.AutoSize = true;
        correctOptionLabel.Location = new Point(430, 29);
        correctOptionLabel.Text = "正確答案";
        correctOptionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        correctOptionComboBox.Items.AddRange(new object[] { "A", "B", "C", "D" });
        correctOptionComboBox.Location = new Point(500, 25);
        correctOptionComboBox.Name = "correctOptionComboBox";
        correctOptionComboBox.SelectedIndex = 1;
        correctOptionComboBox.Size = new Size(65, 23);
        answerDurationLabel.AutoSize = true;
        answerDurationLabel.Location = new Point(580, 29);
        answerDurationLabel.Text = "作答秒數";
        answerDurationNumeric.Location = new Point(650, 25);
        answerDurationNumeric.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
        answerDurationNumeric.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
        answerDurationNumeric.Name = "answerDurationNumeric";
        answerDurationNumeric.Size = new Size(60, 23);
        answerDurationNumeric.Value = new decimal(new int[] { 20, 0, 0, 0 });
        createQuestionButton.Location = new Point(430, 62);
        createQuestionButton.Name = "createQuestionButton";
        createQuestionButton.Size = new Size(130, 58);
        createQuestionButton.Text = "建立題目";
        createQuestionButton.UseVisualStyleBackColor = true;
        createQuestionButton.Click += createQuestionButton_Click;
        //
        // quiz status
        //
        currentQuestionLabel.AutoEllipsis = true;
        currentQuestionLabel.Location = new Point(15, 145);
        currentQuestionLabel.Name = "currentQuestionLabel";
        currentQuestionLabel.Size = new Size(785, 22);
        currentQuestionLabel.Text = "目前題目：等待建立或開始題目";
        quizStateLabel.AutoSize = true;
        quizStateLabel.Location = new Point(15, 177);
        quizStateLabel.Name = "quizStateLabel";
        quizStateLabel.Text = "狀態：Waiting";
        quizProgressLabel.AutoSize = true;
        quizProgressLabel.Location = new Point(190, 177);
        quizProgressLabel.Name = "quizProgressLabel";
        quizProgressLabel.Text = "已作答 0 / 0　在線 0";
        startQuestionButton.Enabled = false;
        startQuestionButton.Location = new Point(430, 220);
        startQuestionButton.Name = "startQuestionButton";
        startQuestionButton.Size = new Size(110, 35);
        startQuestionButton.Text = "開始題目";
        startQuestionButton.UseVisualStyleBackColor = true;
        startQuestionButton.Click += startQuestionButton_Click;
        closeQuestionButton.Enabled = false;
        closeQuestionButton.Location = new Point(550, 220);
        closeQuestionButton.Name = "closeQuestionButton";
        closeQuestionButton.Size = new Size(110, 35);
        closeQuestionButton.Text = "關閉作答";
        closeQuestionButton.UseVisualStyleBackColor = true;
        closeQuestionButton.Click += closeQuestionButton_Click;
        revealAnswerButton.Enabled = false;
        revealAnswerButton.Location = new Point(670, 220);
        revealAnswerButton.Name = "revealAnswerButton";
        revealAnswerButton.Size = new Size(130, 35);
        revealAnswerButton.Text = "公布正確答案";
        revealAnswerButton.UseVisualStyleBackColor = true;
        revealAnswerButton.Click += revealAnswerButton_Click;
        //
        // HostDashboardForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(865, 735);
        Controls.Add(quizGroupBox);
        Controls.Add(participantGrid);
        Controls.Add(onlineCountLabel);
        Controls.Add(statusLabel);
        Controls.Add(connectButton);
        Controls.Add(hostTokenTextBox);
        Controls.Add(hostTokenLabel);
        Controls.Add(eventIdTextBox);
        Controls.Add(eventIdLabel);
        Controls.Add(createEventButton);
        Controls.Add(eventDatePicker);
        Controls.Add(eventDateLabel);
        Controls.Add(eventNameTextBox);
        Controls.Add(eventNameLabel);
        Controls.Add(serverUrlTextBox);
        Controls.Add(serverUrlLabel);
        MinimumSize = new Size(760, 700);
        Name = "HostDashboardForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EventHub Host Console";
        FormClosed += HostDashboardForm_FormClosed;
        ((System.ComponentModel.ISupportInitialize)participantGrid).EndInit();
        quizGroupBox.ResumeLayout(false);
        quizGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)answerDurationNumeric).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label serverUrlLabel;
    private TextBox serverUrlTextBox;
    private Label eventNameLabel;
    private TextBox eventNameTextBox;
    private Label eventDateLabel;
    private DateTimePicker eventDatePicker;
    private Button createEventButton;
    private Label eventIdLabel;
    private TextBox eventIdTextBox;
    private Label hostTokenLabel;
    private TextBox hostTokenTextBox;
    private Button connectButton;
    private Label statusLabel;
    private Label onlineCountLabel;
    private DataGridView participantGrid;
    private DataGridViewTextBoxColumn nameColumn;
    private DataGridViewTextBoxColumn employeeNumberColumn;
    private DataGridViewTextBoxColumn departmentColumn;
    private DataGridViewTextBoxColumn tableNumberColumn;
    private DataGridViewTextBoxColumn onlineColumn;
    private DataGridViewTextBoxColumn scoreColumn;
    private GroupBox quizGroupBox;
    private Label questionTextLabel;
    private TextBox questionTextBox;
    private Label optionALabel;
    private TextBox optionATextBox;
    private Label optionBLabel;
    private TextBox optionBTextBox;
    private Label optionCLabel;
    private TextBox optionCTextBox;
    private Label optionDLabel;
    private TextBox optionDTextBox;
    private Label correctOptionLabel;
    private ComboBox correctOptionComboBox;
    private Label answerDurationLabel;
    private NumericUpDown answerDurationNumeric;
    private Button createQuestionButton;
    private Label currentQuestionLabel;
    private Label quizStateLabel;
    private Label quizProgressLabel;
    private Button startQuestionButton;
    private Button closeQuestionButton;
    private Button revealAnswerButton;
}
