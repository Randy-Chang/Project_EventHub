using EventHub.Domain.Events;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Infrastructure.Tests;

public sealed class EventPersistenceTests
{
    [Fact]
    public void JoinCodeGenerator_ReturnsRequiredFormat()
    {
        var generator = new CryptographicEventJoinCodeGenerator();

        for (var index = 0; index < 100; index++)
        {
            var code = generator.Generate();
            Assert.Equal(EventJoinCode.Length, code.Length);
            Assert.All(code, character => Assert.Contains(character, EventJoinCode.AllowedCharacters));
            Assert.DoesNotContain(code, char.IsWhiteSpace);
        }
    }

    [Fact]
    public async Task JoinCode_IsUniqueCaseInsensitiveAndPersistsAfterContextRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"eventhub-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<EventHubDbContext>()
            .UseSqlite($"Data Source={databasePath};Foreign Keys=True")
            .Options;

        try
        {
            Guid eventId;
            await using (var firstContext = new EventHubDbContext(options))
            {
                await firstContext.Database.EnsureCreatedAsync();
                var eventItem = CreateEvent("First", "ABC234");
                eventId = eventItem.Id;
                firstContext.Events.Add(eventItem);
                await firstContext.SaveChangesAsync();

                firstContext.Events.Add(CreateEvent("Duplicate", "abc234"));
                await Assert.ThrowsAsync<DbUpdateException>(() => firstContext.SaveChangesAsync());
            }

            await using (var restartedContext = new EventHubDbContext(options))
            {
                var reloaded = await restartedContext.Events.AsNoTracking().SingleAsync(item => item.Id == eventId);
                Assert.Equal("ABC234", reloaded.JoinCode);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task AddEventJoinCodeMigration_BackfillsExistingEvents()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"eventhub-migration-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<EventHubDbContext>()
            .UseSqlite($"Data Source={databasePath};Foreign Keys=True")
            .Options;

        try
        {
            await using (var legacyContext = new EventHubDbContext(options))
            {
                var migrator = legacyContext.Database.GetService<IMigrator>();
                await migrator.MigrateAsync("20260822025812_AddQuizScoring");
                await legacyContext.Database.ExecuteSqlRawAsync(
                    """
                    INSERT INTO Events
                        (Id, Name, EventDateUtc, State, IsJoinOpen, HostCredentialHash, CreatedAtUtc, Version)
                    VALUES
                        ({0}, 'Legacy Event', 0, 'Draft', 1, 'legacy-host-hash', 0, 1);
                    """,
                    Guid.NewGuid());
                await migrator.MigrateAsync();
            }

            await using (var upgradedContext = new EventHubDbContext(options))
            {
            var reloaded = await upgradedContext.Events.AsNoTracking().SingleAsync();
            Assert.Equal(EventJoinCode.Length, reloaded.JoinCode.Length);
            Assert.Equal(DisplayMode.Waiting, reloaded.DisplayMode);
                Assert.All(
                    reloaded.JoinCode,
                    character => Assert.Contains(character, EventJoinCode.AllowedCharacters));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    private static DomainEvent CreateEvent(string name, string joinCode) =>
        DomainEvent.Create(
            name,
            joinCode,
            new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero),
            "host-credential-hash",
            new DateTimeOffset(2026, 8, 22, 0, 0, 0, TimeSpan.Zero));
}
