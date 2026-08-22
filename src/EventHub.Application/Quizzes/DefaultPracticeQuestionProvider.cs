using System.Text;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public sealed record DefaultPracticeQuestion(
    string QuestionKey,
    string Text,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    int DurationSeconds,
    string Explanation,
    QuizQuestionMode Mode);

public sealed class DefaultPracticeQuestionProvider
{
    public const string QuizTitle = "EventHub 內建操作練習";

    private static readonly IReadOnlyList<DefaultPracticeQuestion> Questions =
    [
        new(
            "DEFAULT-P001",
            "1 + 1 等於多少？",
            ["1", "2", "3", "4"],
            1,
            10,
            "如果看到這個結果，代表你已完成一次作答。",
            QuizQuestionMode.Practice),
        new(
            "DEFAULT-P002",
            "今天參加的是什麼活動？",
            ["尾牙", "期中考", "駕照考試", "股東會"],
            0,
            10,
            "這是一題操作練習，不列入正式排行榜。",
            QuizQuestionMode.Practice)
    ];

    public IReadOnlyList<DefaultPracticeQuestion> GetQuestions() => Questions;
}

public static class QuestionBankCsvTemplate
{
    public const string Content =
        "QuestionKey,QuizTitle,Category,Difficulty,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds,Explanation,Mode\r\n" +
        "P001,2026尾牙知識王,操作教學,Easy,1,1+1等於多少？,1,2,3,4,B,10,如果看到這個結果代表你已完成一次作答。,Practice\r\n" +
        "P002,2026尾牙知識王,操作教學,Easy,2,今天參加的是什麼活動？,尾牙,期中考,駕照考試,股東會,A,10,這是一題操作練習，不列入正式排行榜。,Practice\r\n" +
        "Q001,2026尾牙知識王,公司歷史,Easy,3,公司成立於哪一年？,1998,2000,2002,2004,B,10,公司成立於2000年。,Scored\r\n" +
        "Q002,2026尾牙知識王,趣味題,Easy,4,\"哪個選項包含逗號,但仍應正確解析？\",\"A,選項\",B,C,D,A,12,此題用於示範CSV引號與逗號處理。,Scored\r\n" +
        "Q003,2026尾牙知識王,產品知識,Medium,5,下列何者為公司產品？,產品A,產品B,產品C,產品D,C,15,此題示範中等難度與說明欄位。,Scored\r\n";

    public static byte[] CreateUtf8BomBytes() =>
        Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Content)).ToArray();
}
