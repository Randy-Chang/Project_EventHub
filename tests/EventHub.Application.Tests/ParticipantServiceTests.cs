using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Domain.Participants;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Tests;

public sealed class ParticipantServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 1, 2, 3, TimeSpan.Zero);

    [Fact]
    public async Task Join_WithExistingSessionToken_ReturnsSameParticipant()
    {
        var fixture = CreateFixture();
        const string clientGeneratedToken = "client-generated-token-with-at-least-32-characters";
        var first = await fixture.Service.JoinAsync(
            new JoinParticipantCommand(
                fixture.EventId,
                "Amy",
                null,
                "A001",
                "IT",
                "1",
                clientGeneratedToken),
            CancellationToken.None);

        var restored = await fixture.Service.JoinAsync(
            new JoinParticipantCommand(fixture.EventId, "Ignored", null, null, null, null, clientGeneratedToken),
            CancellationToken.None);

        Assert.True(first.IsNew);
        Assert.False(restored.IsNew);
        Assert.Equal(first.Participant.Id, restored.Participant.Id);
        Assert.Single(fixture.Participants.Items);
    }

    [Fact]
    public async Task Join_WithoutSessionAndDuplicateEmployeeNumber_IsRejected()
    {
        var fixture = CreateFixture();
        await fixture.Service.JoinAsync(
            new JoinParticipantCommand(fixture.EventId, "Amy", null, "A001", null, null, null),
            CancellationToken.None);

        await Assert.ThrowsAsync<EventHub.Domain.Common.DomainValidationException>(() =>
            fixture.Service.JoinAsync(
                new JoinParticipantCommand(fixture.EventId, "Other", null, "a001", null, null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Join_ExistingSession_WhenJoinIsClosed_RestoresParticipant()
    {
        var fixture = CreateFixture();
        const string token = "existing-participant-token-with-32-characters";
        var first = await fixture.Service.JoinAsync(
            new JoinParticipantCommand(fixture.EventId, "Amy", null, null, null, null, token),
            CancellationToken.None);
        fixture.Event.SetJoinOpen(false);

        var restored = await fixture.Service.JoinAsync(
            new JoinParticipantCommand(fixture.EventId, "Ignored", null, null, null, null, token),
            CancellationToken.None);

        Assert.False(restored.IsNew);
        Assert.Equal(first.Participant.Id, restored.Participant.Id);
    }

    [Fact]
    public async Task Join_NewParticipant_WhenJoinIsClosed_IsRejected()
    {
        var fixture = CreateFixture();
        fixture.Event.SetJoinOpen(false);

        await Assert.ThrowsAsync<EventHub.Domain.Common.DomainValidationException>(() =>
            fixture.Service.JoinAsync(
                new JoinParticipantCommand(fixture.EventId, "New", null, null, null, null, null),
                CancellationToken.None));
    }

    private static Fixture CreateFixture()
    {
        var credentialService = new FakeCredentialService();
        var eventRepository = new FakeEventRepository();
        var participantRepository = new FakeParticipantRepository();
        var presenceStore = new FakePresenceStore();
        var eventItem = DomainEvent.Create(
            "Test Event",
            "TEST23",
            Now.AddDays(1),
            credentialService.HashToken("host-token"),
            Now);
        eventItem.MarkReady();
        eventItem.SetJoinOpen(true);
        eventRepository.Item = eventItem;
        var eventService = new EventService(
            eventRepository,
            credentialService,
            new FixedJoinCodeGenerator(),
            new EventJoinUrlBuilder("http://192.168.1.100:5000"),
            new FixedTimeProvider(Now));
        var service = new ParticipantService(
            participantRepository,
            presenceStore,
            credentialService,
            eventService,
            new FixedTimeProvider(Now));
        return new Fixture(eventItem.Id, eventItem, service, participantRepository);
    }

    private sealed record Fixture(
        Guid EventId,
        DomainEvent Event,
        ParticipantService Service,
        FakeParticipantRepository Participants);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeCredentialService : ICredentialService
    {
        private int sequence;

        public string GenerateToken() => $"token-{++sequence:D4}-{new string('x', 24)}";

        public string HashToken(string token) => $"hash:{token}";

        public bool Matches(string token, string expectedHash) => HashToken(token) == expectedHash;
    }

    private sealed class FixedJoinCodeGenerator : IEventJoinCodeGenerator
    {
        public string Generate() => "TEST23";
    }

    private sealed class FakeEventRepository : IEventRepository
    {
        public DomainEvent? Item { get; set; }

        public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Item?.Id == eventId ? Item : null);
        }

        public Task<DomainEvent?> GetByJoinCodeAsync(
            string normalizedJoinCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(Item?.JoinCode == normalizedJoinCode ? Item : null);

        public Task<bool> JoinCodeExistsAsync(
            string normalizedJoinCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(Item?.JoinCode == normalizedJoinCode);

        public Task<bool> TryAddAsync(DomainEvent eventItem, CancellationToken cancellationToken)
        {
            Item = eventItem;
            return Task.FromResult(true);
        }

        public Task UpdateAsync(DomainEvent eventItem, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        public List<Participant> Items { get; } = [];

        public Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(item => item.Id == participantId));
        }

        public Task<Participant?> GetBySessionHashAsync(
            Guid eventId,
            string sessionCredentialHash,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(item =>
                item.EventId == eventId && item.SessionCredentialHash == sessionCredentialHash));
        }

        public Task<Participant?> GetByEmployeeNumberAsync(
            Guid eventId,
            string normalizedEmployeeNumber,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(item =>
                item.EventId == eventId && item.NormalizedEmployeeNumber == normalizedEmployeeNumber));
        }

        public Task<IReadOnlyList<Participant>> ListByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<Participant> result = Items.Where(item => item.EventId == eventId).ToArray();
            return Task.FromResult(result);
        }

        public Task<int> CountByEventAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.Count(item => item.EventId == eventId));
        }

        public Task AddAsync(Participant participant, CancellationToken cancellationToken)
        {
            Items.Add(participant);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Participant participant, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePresenceStore : IParticipantPresenceStore
    {
        public PresenceChange Connect(Guid eventId, Guid participantId, string connectionId)
            => new(eventId, participantId, true);

        public PresenceChange? Disconnect(string connectionId) => null;

        public IReadOnlySet<Guid> GetOnlineParticipantIds(Guid eventId) => new HashSet<Guid>();
    }
}
