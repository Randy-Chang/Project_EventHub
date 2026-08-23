#nullable disable

namespace EventHub.Host;

partial class QuestionBankImportPreviewForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        fileNameCaptionLabel = new Label();
        fileNameValueLabel = new Label();
        summaryLabel = new Label();
        questionGrid = new DataGridView();
        issueGrid = new DataGridView();
        confirmButton = new Button();
        cancelButton = new Button();
        ((System.ComponentModel.ISupportInitialize)questionGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)issueGrid).BeginInit();
        SuspendLayout();
        fileNameCaptionLabel.AutoSize = true;
        fileNameCaptionLabel.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
        fileNameCaptionLabel.Location = new Point(16, 16);
        fileNameCaptionLabel.Name = "fileNameCaptionLabel";
        fileNameCaptionLabel.Text = "檔案：";
        fileNameValueLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        fileNameValueLabel.AutoEllipsis = true;
        fileNameValueLabel.Location = new Point(66, 16);
        fileNameValueLabel.Name = "fileNameValueLabel";
        fileNameValueLabel.Size = new Size(818, 22);
        summaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        summaryLabel.Location = new Point(16, 44);
        summaryLabel.Name = "summaryLabel";
        summaryLabel.Size = new Size(868, 24);
        questionGrid.AllowUserToAddRows = false;
        questionGrid.AllowUserToDeleteRows = false;
        questionGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        questionGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        questionGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        questionGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "順序" },
            new DataGridViewTextBoxColumn { HeaderText = "Key" },
            new DataGridViewTextBoxColumn { HeaderText = "分類" },
            new DataGridViewTextBoxColumn { HeaderText = "難度" },
            new DataGridViewTextBoxColumn { HeaderText = "模式" },
            new DataGridViewTextBoxColumn { HeaderText = "題目", FillWeight = 220 },
            new DataGridViewTextBoxColumn { HeaderText = "答案" },
            new DataGridViewTextBoxColumn { HeaderText = "秒數" },
            new DataGridViewTextBoxColumn { HeaderText = "答案說明", FillWeight = 160 });
        questionGrid.Location = new Point(16, 76);
        questionGrid.Name = "questionGrid";
        questionGrid.ReadOnly = true;
        questionGrid.RowHeadersVisible = false;
        questionGrid.Size = new Size(868, 252);
        issueGrid.AllowUserToAddRows = false;
        issueGrid.AllowUserToDeleteRows = false;
        issueGrid.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        issueGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        issueGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "等級" },
            new DataGridViewTextBoxColumn { HeaderText = "列" },
            new DataGridViewTextBoxColumn { HeaderText = "Key" },
            new DataGridViewTextBoxColumn { HeaderText = "欄位" },
            new DataGridViewTextBoxColumn { HeaderText = "訊息", FillWeight = 260 });
        issueGrid.Location = new Point(16, 342);
        issueGrid.Name = "issueGrid";
        issueGrid.ReadOnly = true;
        issueGrid.RowHeadersVisible = false;
        issueGrid.Size = new Size(868, 180);
        confirmButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        confirmButton.DialogResult = DialogResult.OK;
        confirmButton.Location = new Point(784, 538);
        confirmButton.Name = "confirmButton";
        confirmButton.Size = new Size(100, 34);
        confirmButton.Text = "確認匯入";
        cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(674, 538);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(100, 34);
        cancelButton.Text = "取消";
        AcceptButton = confirmButton;
        CancelButton = cancelButton;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(900, 588);
        Controls.Add(fileNameCaptionLabel);
        Controls.Add(fileNameValueLabel);
        Controls.Add(summaryLabel);
        Controls.Add(questionGrid);
        Controls.Add(issueGrid);
        Controls.Add(confirmButton);
        Controls.Add(cancelButton);
        MinimumSize = new Size(760, 520);
        Name = "QuestionBankImportPreviewForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "題庫 CSV 預覽";
        ((System.ComponentModel.ISupportInitialize)questionGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)issueGrid).EndInit();
        ResumeLayout(false);
    }

    private Label fileNameCaptionLabel;
    private Label fileNameValueLabel;
    private Label summaryLabel;
    private DataGridView questionGrid;
    private DataGridView issueGrid;
    private Button confirmButton;
    private Button cancelButton;
}
