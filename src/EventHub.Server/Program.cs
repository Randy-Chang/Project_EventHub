using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Presence;
using EventHub.Infrastructure.Security;
using EventHub.Server.Endpoints;
using EventHub.Server.Hubs;
using EventHub.Server.Middleware;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

var dataDirectory = Path.GetFullPath(
    builder.Configuration["EventHub:DataDirectory"] ?? "Data",
    builder.Environment.ContentRootPath);
Directory.CreateDirectory(dataDirectory);

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = Path.Combine(dataDirectory, "eventhub.db"),
    ForeignKeys = true,
    DefaultTimeout = 30
}.ToString();

builder.Services.AddDbContext<EventHubDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSignalR();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ICredentialService, CryptographicCredentialService>();
builder.Services.AddSingleton<IParticipantPresenceStore, InMemoryParticipantPresenceStore>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<ParticipantPresenceService>();
builder.Services.AddScoped<QuizService>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseStaticFiles();

app.MapGet("/", (HttpRequest request) =>
    Results.Redirect($"/_content/EventHub.Web/index.html{request.QueryString}"));
app.MapEventEndpoints();
app.MapParticipantEndpoints();
app.MapQuizEndpoints();
app.MapHub<PresenceHub>("/hubs/event");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();
    await dbContext.Database.MigrateAsync();
    await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
    await dbContext.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=30000;");
}

await app.RunAsync();
