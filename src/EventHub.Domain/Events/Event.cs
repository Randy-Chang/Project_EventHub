using EventHub.Domain.Common;

namespace EventHub.Domain.Events;

public sealed class Event
{
    private Event()
    {
    }

    private Event(
        Guid id,
        string name,
        string joinCode,
        DateTimeOffset eventDateUtc,
        string hostCredentialHash,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = name;
        JoinCode = joinCode;
        EventDateUtc = eventDateUtc;
        HostCredentialHash = hostCredentialHash;
        CreatedAtUtc = createdAtUtc;
        State = EventState.Draft;
        DisplayMode = DisplayMode.Waiting;
        IsJoinOpen = false;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string JoinCode { get; private set; } = string.Empty;

    public DateTimeOffset EventDateUtc { get; private set; }

    public EventState State { get; private set; }

    public DisplayMode DisplayMode { get; private set; }

    public bool IsJoinOpen { get; private set; }

    public string HostCredentialHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public long Version { get; private set; }

    public static Event Create(
        string name,
        string joinCode,
        DateTimeOffset eventDateUtc,
        string hostCredentialHash,
        DateTimeOffset createdAtUtc)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new DomainValidationException("活動名稱不可為空白。");
        }

        if (normalizedName.Length > 200)
        {
            throw new DomainValidationException("活動名稱不可超過 200 個字元。");
        }

        if (string.IsNullOrWhiteSpace(hostCredentialHash))
        {
            throw new DomainValidationException("Host credential 不可為空白。");
        }

        return new Event(
            Guid.NewGuid(),
            normalizedName,
            EventJoinCode.Normalize(joinCode),
            eventDateUtc,
            hostCredentialHash,
            createdAtUtc);
    }

    public void SetJoinOpen(bool isOpen)
    {
        if (State == EventState.Completed)
        {
            throw new DomainValidationException("已完成的活動不可變更加入設定。");
        }

        if (isOpen && State == EventState.Draft)
        {
            throw new DomainValidationException("活動就緒後才能開放加入。");
        }

        IsJoinOpen = isOpen;
        Version++;
    }

    public void SetDisplayMode(DisplayMode displayMode)
    {
        DisplayMode = displayMode;
        Version++;
    }

    public void MarkReady()
    {
        if (State != EventState.Draft)
        {
            throw new DomainValidationException("只有草稿中的活動可以標記為就緒。");
        }

        State = EventState.Ready;
        Version++;
    }

    public void Activate()
    {
        if (State != EventState.Ready)
        {
            throw new DomainValidationException("只有已就緒的活動可以開始。");
        }

        State = EventState.Active;
        Version++;
    }

    public void Complete()
    {
        if (State != EventState.Active)
        {
            throw new DomainValidationException("只有進行中的活動可以完成。");
        }

        State = EventState.Completed;
        IsJoinOpen = false;
        Version++;
    }
}
