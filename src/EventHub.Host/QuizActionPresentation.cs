namespace EventHub.Host;

internal enum QuizPrimaryAction
{
    None,
    Start,
    Close,
    Reveal,
    Next,
    StartOfficial,
    FinalLeaderboard
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
        QuizQuestionMode? mode,
        bool hasNextQuestionInMode,
        bool hasScoredQuestions) => state switch
        {
            QuizState.Waiting => new(
                hasSelectedQuestion ? QuizPrimaryAction.Start : QuizPrimaryAction.None,
                hasSelectedQuestion ? "開始本題" : "請先選擇題庫",
                hasSelectedQuestion,
                hasSelectedQuestion ? "題目已就緒，可以開放作答。" : "請先匯入、選擇題庫，或執行內建熱身。"),
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
                mode == QuizQuestionMode.Practice && !hasNextQuestionInMode
                    ? QuizPrimaryAction.StartOfficial
                    : mode == QuizQuestionMode.Scored && !hasNextQuestionInMode
                        ? QuizPrimaryAction.FinalLeaderboard
                        : QuizPrimaryAction.Next,
                mode == QuizQuestionMode.Practice && !hasNextQuestionInMode
                    ? "開始正式比賽"
                    : mode == QuizQuestionMode.Scored && !hasNextQuestionInMode
                        ? "查看最終排行榜"
                        : "下一題",
                mode != QuizQuestionMode.Practice || hasNextQuestionInMode || hasScoredQuestions,
                mode == QuizQuestionMode.Practice && !hasNextQuestionInMode
                    ? hasScoredQuestions ? "操作練習完成；準備進入正式比賽。" : "操作練習完成，但題庫沒有正式題。"
                    : mode == QuizQuestionMode.Scored && !hasNextQuestionInMode
                        ? "正式題目已完成，可以顯示最終排行榜。"
                        : "本題已完成，可以前往下一題。"),
            _ => new(QuizPrimaryAction.None, "等待題目", false, "等待 Server 狀態。")
        };
}
