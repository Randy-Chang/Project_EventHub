using EventHub.Host;
using System.Windows.Forms;

namespace EventHub.Host.Tests;

public sealed class QuestionBankImportPreviewFormTests
{
    [Fact]
    public void InvalidPreview_ShowsFileNameAndDisablesImport()
    {
        var preview = new QuestionBankPreviewView(
            "annual-party.csv",
            "尾牙知識王",
            0,
            [],
            [new QuestionBankIssueView(2, "Q001", "Question", "題目不可空白。", QuestionBankIssueSeverity.Error)],
            false,
            1,
            0);

        using var form = new QuestionBankImportPreviewForm(preview);
        var fileName = Assert.IsType<Label>(Assert.Single(form.Controls.Find("fileNameValueLabel", true)));
        var import = Assert.IsType<Button>(Assert.Single(form.Controls.Find("confirmButton", true)));

        Assert.Equal("annual-party.csv", fileName.Text);
        Assert.False(import.Enabled);
    }
}
