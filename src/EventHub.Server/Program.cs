using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Display;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Csv;
using EventHub.Infrastructure.Presence;
using EventHub.Infrastructure.Security;
using EventHub.Server.Endpoints;
using EventHub.Server.Hubs;
using EventHub.Server.Middleware;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.WebHost.UseStaticWebAssets();

var joinUrlBuilder = new EventJoinUrlBuilder(
    builder.Configuration["EventHub:JoinBaseUrl"]
    ?? throw new InvalidOperationException("缺少 EventHub:JoinBaseUrl 設定。"));

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
builder.Services.AddSingleton<IEventJoinCodeGenerator, CryptographicEventJoinCodeGenerator>();
builder.Services.AddSingleton(joinUrlBuilder);
builder.Services.AddSingleton<IParticipantPresenceStore, InMemoryParticipantPresenceStore>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
builder.Services.AddScoped<IQuestionBankCsvParser, CsvHelperQuestionBankParser>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<ParticipantPresenceService>();
builder.Services.AddSingleton<QuizScoreCalculator>();
builder.Services.AddScoped<QuizScoringService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<QuestionBankService>();
builder.Services.AddSingleton<QuestionBankValidator>();
builder.Services.AddSingleton<DefaultPracticeQuestionProvider>();
builder.Services.AddScoped<DisplayService>();
builder.Services.AddScoped<HostSessionService>();
builder.Services.AddScoped<EventRecoveryService>();

var app = builder.Build();

if (joinUrlBuilder.IsLoopback)
{
    app.Logger.LogWarning(
        "EventHub JoinBaseUrl {JoinBaseUrl} 使用 localhost/loopback，其他手機無法連線。",
        joinUrlBuilder.JoinBaseUrl);
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseStaticFiles();

app.MapGet("/", (HttpRequest request) =>
    Results.Redirect($"/_content/EventHub.Web/index.html{request.QueryString}"));
app.MapGet("/join/{joinCode}", (string joinCode) =>
    Results.Redirect(
        $"/_content/EventHub.Web/index.html?joinCode={Uri.EscapeDataString(joinCode)}"));
app.MapEventEndpoints();
app.MapParticipantEndpoints();
app.MapQuizEndpoints();
app.MapQuestionBankEndpoints();
app.MapDisplayEndpoints();
app.MapNetworkEndpoints();
app.MapHub<PresenceHub>("/hubs/event");
app.MapGet("/health", (HttpRequest request) => Results.Ok(new
{
    status = "ok",
    serverTimeUtc = DateTimeOffset.UtcNow,
    version = typeof(Program).Assembly.GetName().Version?.ToString(),
    requestBaseUrl = $"{request.Scheme}://{request.Host}"
}));

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();
    await dbContext.Database.MigrateAsync();
    await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
    await dbContext.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=30000;");
    var recoveryService = scope.ServiceProvider.GetRequiredService<EventRecoveryService>();
    var recoveredSessions = await recoveryService.RecoverExpiredQuestionsAsync(CancellationToken.None);
    foreach (var sessionId in recoveredSessions)
    {
        app.Logger.LogWarning(
            "Recovered expired question session {QuestionSessionId} as Closed during server startup.",
            sessionId);
    }
}

await app.RunAsync();
