using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Domain.Events;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Tests;

public sealed class EventServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_GeneratesPersistsAndReturnsJoinInformation()
    {
        var fixture = CreateFixture("8k3f2a");

        var result = await fixture.Service.CreateAsync(
            new CreateEventCommand("Annual Party", Now.AddDays(1)),
            CancellationToken.None);

        var persisted = Assert.Single(fixture.Repository.Items);
        Assert.Equal("8K3F2A", persisted.JoinCode);
        Assert.Equal("8K3F2A", result.Event.JoinCode);
        Assert.Equal("8K3F2A", result.JoinInfo.JoinCode);
        Assert.Equal("http://192.168.10.100:5000/join/8K3F2A", result.JoinInfo.JoinUrl);
    }

    [Fact]
    public async Task Create_WhenApplicationDetectsCollision_Retries()
    {
        var fixture = CreateFixture("ABC234", "DEF234");
        fixture.Repository.Items.Add(CreateEvent("Existing", "ABC234"));

        var result = await fixture.Service.CreateAsync(
            new CreateEventCommand("New Event", Now),
            CancellationToken.None);

        Assert.Equal("DEF234", result.Event.JoinCode);
        Assert.Equal(2, fixture.Generator.CallCount);
    }

    [Fact]
    public async Task Create_WhenDatabaseReportsConcurrentCollision_Retries()
    {
        var fixture = CreateFixture("ABC234", "DEF234");
        fixture.Repository.RejectNextAdd = true;

        var result = await fixture.Service.CreateAsync(
            new CreateEventCommand("New Event", Now),
            CancellationToken.None);

        Assert.Equal("DEF234", result.Event.JoinCode);
        Assert.Equal(2, fixture.Generator.CallCount);
    }

    [Fact]
    public async Task Create_AfterTenCollisions_ThrowsClearException()
    {
        var fixture = CreateFixture(Enumerable.Repeat("ABC234", 10).ToArray());
        fixture.Repository.Items.Add(CreateEvent("Existing", "ABC234"));

        await Assert.ThrowsAsync<EventJoinCodeGenerationException>(() => fixture.Service.CreateAsync(
            new CreateEventCommand("New Event", Now),
            CancellationToken.None));

        Assert.Equal(10, fixture.Generator.CallCount);
        Assert.Single(fixture.Repository.Items);
    }

    [Fact]
    public async Task ResolveJoinCode_IsCaseInsensitiveAndReturnsAvailability()
    {
        var fixture = CreateFixture("ABC234");
        var eventItem = CreateEvent("Family Day", "ABC234");
        fixture.Repository.Items.Add(eventItem);

        var result = await fixture.Service.ResolveJoinCodeAsync(" abc234 ", CancellationToken.None);

        Assert.Equal(eventItem.Id, result.EventId);
        Assert.True(result.IsJoinOpen);
    }

    [Fact]
    public async Task ResolveJoinCode_ClosedEventReturnsUnavailable()
    {
        var fixture = CreateFixture("ABC234");
        var eventItem = CreateEvent("Closed", "ABC234");
        eventItem.SetJoinOpen(false);
        fixture.Repository.Items.Add(eventItem);

        var result = await fixture.Service.ResolveJoinCodeAsync("ABC234", CancellationToken.None);

        Assert.False(result.IsJoinOpen);
    }

    [Fact]
    public async Task ResolveJoinCode_UnknownCodeThrowsNotFound()
    {
        var fixture = CreateFixture("ABC234");

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.Service.ResolveJoinCodeAsync("ABC234", CancellationToken.None));
    }

    [Theory]
    [InlineData("http://192.168.10.100:5000")]
    [InlineData("http://192.168.10.100:5000/")]
    public void JoinUrlBuilder_NormalizesTrailingSlash(string baseUrl)
    {
        var builder = new EventJoinUrlBuilder(baseUrl);

        Assert.Equal("http://192.168.10.100:5000/join/8K3F2A", builder.Build("8k3f2a"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.1.10:5000")]
    [InlineData("ftp://192.168.1.10/join")]
    public void JoinUrlBuilder_RejectsInvalidConfiguration(string value)
    {
        Assert.Throws<InvalidOperationException>(() => new EventJoinUrlBuilder(value));
    }

    [Fact]
    public void JoinUrlBuilder_SetJoinBaseUrl_UpdatesSubsequentJoinInformation()
    {
        var builder = new EventJoinUrlBuilder("http://localhost:5000");

        builder.SetJoinBaseUrl("http://192.168.50.25:5000/");

        Assert.False(builder.IsLoopback);
        Assert.Equal("http://192.168.50.25:5000", builder.JoinBaseUrl);
        Assert.Equal("http://192.168.50.25:5000/join/ABC234", builder.Build("abc234"));
    }

    [Fact]
    public void JoinUrlBuilder_SetInvalidJoinBaseUrl_PreservesPreviousValue()
    {
        var builder = new EventJoinUrlBuilder("http://192.168.50.25:5000");

        Assert.Throws<InvalidOperationException>(() => builder.SetJoinBaseUrl("not-a-url"));

        Assert.Equal("http://192.168.50.25:5000", builder.JoinBaseUrl);
    }

    private static Fixture CreateFixture(params string[] codes)
    {
        var repository = new FakeEventRepository();
        var generator = new SequenceJoinCodeGenerator(codes);
        var service = new EventService(
            repository,
            new FakeCredentialService(),
            generator,
            new EventJoinUrlBuilder("http://192.168.10.100:5000"),
            new FixedTimeProvider(Now));
        return new Fixture(service, repository, generator);
    }

    private static DomainEvent CreateEvent(string name, string joinCode) =>
        DomainEvent.Create(name, joinCode, Now, "host-hash", Now);

    private sealed record Fixture(
        EventService Service,
        FakeEventRepository Repository,
        SequenceJoinCodeGenerator Generator);

    private sealed class SequenceJoinCodeGenerator(params string[] codes) : IEventJoinCodeGenerator
    {
        private readonly Queue<string> remaining = new(codes);

        public int CallCount { get; private set; }

        public string Generate()
        {
            CallCount++;
            return remaining.Dequeue();
        }
    }

    private sealed class FakeCredentialService : ICredentialService
    {
        public string GenerateToken() => "host-token-with-at-least-32-characters";
        public string HashToken(string token) => $"hash:{token}";
        public bool Matches(string token, string expectedHash) => HashToken(token) == expectedHash;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeEventRepository : IEventRepository
    {
        public List<DomainEvent> Items { get; } = [];
        public bool RejectNextAdd { get; set; }

        public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item => item.Id == eventId));

        public Task<DomainEvent?> GetByJoinCodeAsync(
            string normalizedJoinCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item => item.JoinCode == normalizedJoinCode));

        public Task<bool> JoinCodeExistsAsync(
            string normalizedJoinCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(item => item.JoinCode == normalizedJoinCode));

        public Task<bool> TryAddAsync(DomainEvent eventItem, CancellationToken cancellationToken)
        {
            if (RejectNextAdd)
            {
                RejectNextAdd = false;
                return Task.FromResult(false);
            }

            Items.Add(eventItem);
            return Task.FromResult(true);
        }

        public Task UpdateAsync(DomainEvent eventItem, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
