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
        ((System.ComponentModel.ISupportInitialize)participantGrid).BeginInit();
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
        participantGrid.Size = new Size(825, 335);
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
        // HostDashboardForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(865, 525);
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
        MinimumSize = new Size(760, 460);
        Name = "HostDashboardForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EventHub Host Console";
        FormClosed += HostDashboardForm_FormClosed;
        ((System.ComponentModel.ISupportInitialize)participantGrid).EndInit();
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
}
