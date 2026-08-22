#nullable disable

namespace EventHub.Host.Views;

partial class QuestionBankView
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
        commandPanel = new FlowLayoutPanel();
        importButton = new Button();
        exportTemplateButton = new Button();
        refreshButton = new Button();
        questionBankComboBox = new ComboBox();
        previousButton = new Button();
        nextButton = new Button();
        bankSummaryLabel = new Label();
        questionGrid = new DataGridView();
        questionDetailTextBox = new TextBox();
        statusLabel = new Label();
        commandPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)questionGrid).BeginInit();
        SuspendLayout();
        titleLabel.Name = "titleLabel";
        commandPanel.Name = "commandPanel";
        importButton.Name = "importButton";
        exportTemplateButton.Name = "exportTemplateButton";
        refreshButton.Name = "refreshButton";
        questionBankComboBox.Name = "questionBankComboBox";
        previousButton.Name = "previousButton";
        nextButton.Name = "nextButton";
        bankSummaryLabel.Name = "bankSummaryLabel";
        questionGrid.Name = "questionGrid";
        questionDetailTextBox.Name = "questionDetailTextBox";
        statusLabel.Name = "statusLabel";
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
        titleLabel.Height = 58;
        titleLabel.Text = "題庫管理";
        commandPanel.Controls.Add(importButton);
        commandPanel.Controls.Add(exportTemplateButton);
        commandPanel.Controls.Add(refreshButton);
        commandPanel.Controls.Add(questionBankComboBox);
        commandPanel.Controls.Add(previousButton);
        commandPanel.Controls.Add(nextButton);
        commandPanel.Dock = DockStyle.Top;
        commandPanel.Height = 48;
        commandPanel.WrapContents = false;
        importButton.Size = new Size(105, 34);
        importButton.Text = "匯入 CSV";
        importButton.Click += importButton_Click;
        exportTemplateButton.Size = new Size(105, 34);
        exportTemplateButton.Text = "匯出範本";
        exportTemplateButton.Click += exportTemplateButton_Click;
        refreshButton.Size = new Size(90, 34);
        refreshButton.Text = "重新整理";
        refreshButton.Click += refreshButton_Click;
        questionBankComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        questionBankComboBox.Margin = new Padding(16, 5, 4, 3);
        questionBankComboBox.Size = new Size(260, 23);
        questionBankComboBox.SelectedIndexChanged += questionBankComboBox_SelectedIndexChanged;
        previousButton.Size = new Size(85, 34);
        previousButton.Text = "上一題";
        previousButton.Click += previousButton_Click;
        nextButton.Size = new Size(85, 34);
        nextButton.Text = "下一題";
        nextButton.Click += nextButton_Click;
        bankSummaryLabel.Dock = DockStyle.Top;
        bankSummaryLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold);
        bankSummaryLabel.Height = 34;
        bankSummaryLabel.Text = "尚未選擇題庫";
        questionGrid.AllowUserToAddRows = false;
        questionGrid.AllowUserToDeleteRows = false;
        questionGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        questionGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "順序", FillWeight = 45 },
            new DataGridViewTextBoxColumn { HeaderText = "Key", FillWeight = 75 },
            new DataGridViewTextBoxColumn { HeaderText = "分類", FillWeight = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "難度", FillWeight = 55 },
            new DataGridViewTextBoxColumn { HeaderText = "模式", FillWeight = 65 },
            new DataGridViewTextBoxColumn { HeaderText = "題目", FillWeight = 230 },
            new DataGridViewTextBoxColumn { HeaderText = "秒數", FillWeight = 45 },
            new DataGridViewTextBoxColumn { HeaderText = "狀態", FillWeight = 65 });
        questionGrid.Dock = DockStyle.Fill;
        questionGrid.MultiSelect = false;
        questionGrid.ReadOnly = true;
        questionGrid.RowHeadersVisible = false;
        questionGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        questionGrid.SelectionChanged += questionGrid_SelectionChanged;
        questionDetailTextBox.Dock = DockStyle.Bottom;
        questionDetailTextBox.Height = 105;
        questionDetailTextBox.Multiline = true;
        questionDetailTextBox.ReadOnly = true;
        questionDetailTextBox.ScrollBars = ScrollBars.Vertical;
        questionDetailTextBox.Text = "選擇題目後顯示內容。";
        statusLabel.Dock = DockStyle.Bottom;
        statusLabel.Height = 28;
        statusLabel.Text = "等待操作";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Controls.Add(questionGrid);
        Controls.Add(statusLabel);
        Controls.Add(questionDetailTextBox);
        Controls.Add(bankSummaryLabel);
        Controls.Add(commandPanel);
        Controls.Add(titleLabel);
        Name = "QuestionBankView";
        Padding = new Padding(24);
        Size = new Size(984, 728);
        commandPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)questionGrid).EndInit();
        ResumeLayout(false);
    }

    private Label titleLabel;
    private FlowLayoutPanel commandPanel;
    private Button importButton;
    private Button exportTemplateButton;
    private Button refreshButton;
    private ComboBox questionBankComboBox;
    private Button previousButton;
    private Button nextButton;
    private Label bankSummaryLabel;
    private DataGridView questionGrid;
    private TextBox questionDetailTextBox;
    private Label statusLabel;
}
