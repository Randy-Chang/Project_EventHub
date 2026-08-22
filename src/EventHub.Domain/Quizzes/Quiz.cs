using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class Quiz
{
    private Quiz()
    {
    }

    private Quiz(Guid id, Guid eventId, string title, DateTimeOffset createdAtUtc)
    {
        Id = id;
        EventId = eventId;
        Title = title;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Quiz Create(Guid eventId, string title, DateTimeOffset createdAtUtc)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainValidationException("活動識別碼不可為空白。");
        }

        var normalizedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle) || normalizedTitle.Length > 200)
        {
            throw new DomainValidationException("Quiz 名稱須為 1 至 200 個字元。");
        }

        return new Quiz(Guid.NewGuid(), eventId, normalizedTitle, createdAtUtc);
    }
}
