namespace EventHub.Application.Events;

public sealed class EventJoinCodeGenerationException()
    : Exception("無法產生唯一的活動加入碼，請稍後再試。");
