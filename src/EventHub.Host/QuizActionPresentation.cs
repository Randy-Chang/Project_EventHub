namespace EventHub.Host;

internal enum QuizPrimaryAction
{
    None,
    Start,
    Close,
    Reveal,
    Next
}

internal sealed record QuizActionPresentation(
    QuizPrimaryAction Action,
    string ButtonText,
    bool IsEnabled,
    string Guidance)
{
    public static QuizActionPresentation Resolve(
        QuizState state,
        bool hasSelectedQuestion,
        bool hasNextQuestion) => state switch
        {
            QuizState.Waiting => new(
                QuizPrimaryAction.Start,
                "開始本題",
                hasSelectedQuestion,
                hasSelectedQuestion ? "題目已就緒，可以開放作答。" : "請先到題庫選擇題目。"),
            QuizState.Open => new(
                QuizPrimaryAction.Close,
                "關閉作答",
                true,
                "題目正在作答中；確認時機後關閉作答。"),
            QuizState.Closed => new(
                QuizPrimaryAction.Reveal,
                "公布答案",
                true,
                "作答已關閉，下一步公布正確答案。"),
            QuizState.Revealed => new(
                QuizPrimaryAction.Next,
                "下一題",
                hasNextQuestion,
                hasNextQuestion ? "本題已完成，可以前往下一題。" : "本題已完成，題庫中沒有下一題。"),
            _ => new(QuizPrimaryAction.None, "等待題目", false, "等待 Server 狀態。")
        };
}
