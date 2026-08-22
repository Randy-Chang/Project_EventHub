using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EventHub.Application.Quizzes;

namespace EventHub.Infrastructure.Csv;

public sealed class CsvHelperQuestionBankParser : IQuestionBankCsvParser
{
    public async Task<QuestionBankParseResult> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var issues = new List<QuestionBankIssue>();
        var rows = new List<QuestionBankRawRow>();
        if (stream.CanSeek)
        {
            var originalPosition = stream.Position;
            var first = stream.ReadByte();
            var second = stream.ReadByte();
            stream.Position = originalPosition;
            if ((first == 0xFF && second == 0xFE) || (first == 0xFE && second == 0xFF))
            {
                return new QuestionBankParseResult(
                    [],
                    [new QuestionBankIssue(null, null, "Encoding", "CSV 必須使用 UTF-8 編碼。")]);
            }
        }

        try
        {
            using var reader = new StreamReader(
                stream,
                new UTF8Encoding(false, true),
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = args => throw new BadDataException(args.Field, args.RawRecord, args.Context),
                MissingFieldFound = null,
                HeaderValidated = null
            };
            using var csv = new CsvReader(reader, configuration);
            if (!await csv.ReadAsync())
            {
                return new QuestionBankParseResult([], []);
            }

            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? [];
            var headerMap = headers
                .Select((header, index) => new { Header = header.Trim(), Index = index })
                .GroupBy(item => item.Header, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);
            foreach (var required in QuestionBankImportLimits.RequiredHeaders)
            {
                if (!headerMap.ContainsKey(required))
                {
                    issues.Add(new QuestionBankIssue(null, null, required, $"缺少必要欄位：{required}。"));
                }
            }

            if (issues.Count > 0)
            {
                return new QuestionBankParseResult([], issues);
            }

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var values = QuestionBankImportLimits.RequiredHeaders
                    .ToDictionary(
                        header => header,
                        header => csv.GetField(headerMap[header]) ?? string.Empty,
                        StringComparer.OrdinalIgnoreCase);
                if (values.Values.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                rows.Add(new QuestionBankRawRow(
                    csv.Parser.Row,
                    values["QuestionKey"],
                    values["QuizTitle"],
                    values["Category"],
                    values["Difficulty"],
                    values["Order"],
                    values["Question"],
                    values["OptionA"],
                    values["OptionB"],
                    values["OptionC"],
                    values["OptionD"],
                    values["CorrectOption"],
                    values["DurationSeconds"],
                    values["Explanation"],
                    values["Mode"]));
            }
        }
        catch (DecoderFallbackException)
        {
            issues.Add(new QuestionBankIssue(null, null, "Encoding", "CSV 必須使用 UTF-8 編碼。"));
        }
        catch (CsvHelperException exception)
        {
            issues.Add(new QuestionBankIssue(
                exception.Context?.Parser?.Row,
                null,
                "CSV",
                "CSV 格式錯誤，請檢查引號、逗號或欄位換行。"));
        }

        return new QuestionBankParseResult(rows, issues);
    }
}
