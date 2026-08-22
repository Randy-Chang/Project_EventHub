using System.Text;
using EventHub.Infrastructure.Csv;

namespace EventHub.Infrastructure.Tests;

public sealed class QuestionBankCsvParserTests
{
    private const string Header =
        "QuestionKey,QuizTitle,Category,Difficulty,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds,Explanation,Mode\r\n";

    [Fact]
    public async Task Parse_ValidCsv_ReturnsRow() =>
        Assert.Single((await ParseAsync(Header + Row())).Rows);

    [Fact]
    public async Task Parse_Utf8Chinese_PreservesText() =>
        Assert.Equal("公司成立於哪一年？", (await ParseAsync(Header + Row())).Rows.Single().Question);

    [Fact]
    public async Task Parse_Utf8Bom_IsAccepted()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Header + Row())).ToArray();
        Assert.Single((await ParseAsync(bytes)).Rows);
    }

    [Fact]
    public async Task Parse_QuotedComma_IsPreserved()
    {
        var result = await ParseAsync(Header + Row(question: "\"台北,新竹何者是總部？\""));
        Assert.Equal("台北,新竹何者是總部？", result.Rows.Single().Question);
    }

    [Fact]
    public async Task Parse_EscapedQuote_IsPreserved()
    {
        var result = await ParseAsync(Header + Row(question: "\"產品代號 \"\"Alpha\"\"？\""));
        Assert.Equal("產品代號 \"Alpha\"？", result.Rows.Single().Question);
    }

    [Fact]
    public async Task Parse_QuotedNewLine_IsPreserved()
    {
        var result = await ParseAsync(Header + Row(question: "\"第一行\r\n第二行\""));
        Assert.Equal("第一行\r\n第二行", result.Rows.Single().Question);
    }

    [Fact]
    public async Task Parse_ReorderedHeaders_AreAccepted()
    {
        const string csv = "QuizTitle,QuestionKey,Difficulty,Category,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds,Mode,Explanation\r\n尾牙,Q1,Easy,公司,1,題目,A,B,C,D,A,10,Scored,說明\r\n";
        Assert.Equal("Q1", (await ParseAsync(csv)).Rows.Single().QuestionKey);
    }

    [Fact]
    public async Task Parse_UnknownHeader_IsIgnored()
    {
        var csv = Header.TrimEnd('\r', '\n') + ",Unknown\r\n" + Row().TrimEnd('\r', '\n') + ",ignored\r\n";
        Assert.Single((await ParseAsync(csv)).Rows);
    }

    [Fact]
    public async Task Parse_BlankRows_AreIgnored() =>
        Assert.Single((await ParseAsync(Header + "\r\n" + Row() + "\r\n")).Rows);

    [Fact]
    public async Task Parse_MissingRequiredHeader_ReturnsIssue()
    {
        var result = await ParseAsync(Header.Replace("QuestionKey,", string.Empty) + Row());
        Assert.Contains(result.Issues, issue => issue.Field == "QuestionKey");
    }

    [Fact]
    public async Task Parse_NonUtf8_ReturnsEncodingIssue()
    {
        var bytes = new byte[] { 0xFF, 0xFE, 0xFA };
        var result = await ParseAsync(bytes);
        Assert.Contains(result.Issues, issue => issue.Field == "Encoding");
    }

    [Fact]
    public async Task Parse_InvalidQuote_ReturnsCsvIssue()
    {
        var result = await ParseAsync(Header + Row(question: "bad\"quote"));
        Assert.Contains(result.Issues, issue => issue.Field == "CSV");
    }

    [Fact]
    public async Task Parse_ExportTemplate_RoundTripsFiveValidRows()
    {
        var result = await ParseAsync(EventHub.Application.Quizzes.QuestionBankCsvTemplate.CreateUtf8BomBytes());
        var (title, rows, issues) = new EventHub.Application.Quizzes.QuestionBankValidator().Validate(result);

        Assert.Equal("2026尾牙知識王", title);
        Assert.Equal(5, rows.Count);
        Assert.Empty(issues);
        Assert.Equal(2, rows.Count(row => row.Mode == EventHub.Domain.Quizzes.QuizQuestionMode.Practice));
        Assert.Equal(3, rows.Count(row => row.Mode == EventHub.Domain.Quizzes.QuizQuestionMode.Scored));
        Assert.Equal("A,選項", rows.Single(row => row.QuestionKey == "Q002").Options[0]);
    }

    private static string Row(string question = "公司成立於哪一年？") =>
        $"Q1,尾牙題庫,公司,Medium,1,{question},1998,2000,2004,2008,B,20,答案說明,Scored\r\n";

    private static Task<EventHub.Application.Quizzes.QuestionBankParseResult> ParseAsync(string csv) =>
        ParseAsync(Encoding.UTF8.GetBytes(csv));

    private static async Task<EventHub.Application.Quizzes.QuestionBankParseResult> ParseAsync(byte[] bytes)
    {
        await using var stream = new MemoryStream(bytes);
        return await new CsvHelperQuestionBankParser().ParseAsync(stream, CancellationToken.None);
    }
}
