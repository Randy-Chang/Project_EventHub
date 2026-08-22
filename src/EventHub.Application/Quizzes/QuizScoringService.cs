using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public sealed class QuizScoringService(
    IQuizRepository quizRepository,
    EventService eventService,
    ParticipantService participantService,
    QuizScoreCalculator scoreCalculator,
    TimeProvider timeProvider)
{
    public async Task<RevealQuizQuestionResult> RevealAnswerAsync(
        QuizHostCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureHostAuthorizedAsync(command.EventId, command.HostToken, cancellationToken);
        var nowUtc = timeProvider.GetUtcNow();
        Guid? correctOptionId = null;
        var wasChanged = await quizRepository.ExecuteRevealAndScoreAsync(
            command.EventId,
            command.SessionId,
            snapshot =>
            {
                if (snapshot.Session.State != QuizQuestionState.Closed)
                {
                    throw new QuizApplicationException(
                        QuizErrorCode.InvalidQuestionState,
                        "只有已關閉的題目可以公布答案。");
                }

                if (snapshot.Session.StartedAtUtc is null || snapshot.Session.AnswerDeadlineUtc is null)
                {
                    throw new QuizApplicationException(
                        QuizErrorCode.InvalidQuestionState,
                        "題目缺少有效的作答時間，無法計分。");
                }

                correctOptionId = snapshot.Question.CorrectOptionId;
                var scoreByParticipant = snapshot.ParticipantScores.ToDictionary(score => score.ParticipantId);
                var results = new List<ParticipantQuestionResult>(snapshot.Answers.Count);
                var newScores = new List<ParticipantQuizScore>();
                foreach (var answer in snapshot.Answers)
                {
                    var calculation = scoreCalculator.Calculate(
                        answer.SelectedOptionId == snapshot.Question.CorrectOptionId,
                        snapshot.Session.StartedAtUtc.Value,
                        snapshot.Session.AnswerDeadlineUtc.Value,
                        answer.SubmittedAtUtc);
                    var result = ParticipantQuestionResult.Create(
                        snapshot.Session.EventId,
                        snapshot.Session.QuizId,
                        snapshot.Session.Id,
                        answer.ParticipantId,
                        calculation,
                        nowUtc);
                    results.Add(result);

                    if (!scoreByParticipant.TryGetValue(answer.ParticipantId, out var participantScore))
                    {
                        participantScore = ParticipantQuizScore.Create(
                            snapshot.Session.EventId,
                            snapshot.Session.QuizId,
                            answer.ParticipantId,
                            nowUtc);
                        scoreByParticipant.Add(answer.ParticipantId, participantScore);
                        newScores.Add(participantScore);
                    }

                    participantScore.Apply(result, nowUtc);
                }

                snapshot.Session.Reveal(nowUtc);
                return new QuizRevealChanges(results, newScores);
            },
            cancellationToken);

        if (correctOptionId is null)
        {
            var session = await quizRepository.GetCurrentSessionAsync(command.EventId, cancellationToken);
            if (session is null || session.Id != command.SessionId || session.State != QuizQuestionState.Revealed)
            {
                throw new QuizApplicationException(QuizErrorCode.SessionNotFound, "找不到指定的題目場次。");
            }

            var question = await quizRepository.GetQuestionAsync(session.QuestionId, cancellationToken)
                ?? throw new QuizApplicationException(QuizErrorCode.QuestionNotFound, "找不到指定的題目。");
            correctOptionId = question.CorrectOptionId;
        }

        return new RevealQuizQuestionResult(command.SessionId, correctOptionId.Value, !wasChanged);
    }

    public async Task<QuizLeaderboard> GetLeaderboardAsync(
        Guid eventId,
        int top,
        string? hostToken,
        Guid? participantId,
        string? participantToken,
        CancellationToken cancellationToken)
    {
        await AuthorizeViewerAsync(eventId, hostToken, participantId, participantToken, cancellationToken);
        return await GetPublicLeaderboardAsync(eventId, top, cancellationToken);
    }

    public async Task<QuizLeaderboard> GetPublicLeaderboardAsync(
        Guid eventId,
        int top,
        CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByEventAsync(eventId, cancellationToken);
        if (quiz is null)
        {
            return new QuizLeaderboard(eventId, null, []);
        }

        var entries = await BuildLeaderboardAsync(eventId, quiz.Id, cancellationToken);
        return new QuizLeaderboard(eventId, quiz.Id, entries.Take(Math.Clamp(top, 1, 100)).ToArray());
    }

    public async Task<ParticipantQuizScoreSummary> GetMyScoreAsync(
        Guid eventId,
        Guid participantId,
        string participantToken,
        CancellationToken cancellationToken)
    {
        _ = await participantService.ValidateSessionCredentialAsync(
            eventId,
            participantId,
            participantToken,
            cancellationToken);
        var quiz = await quizRepository.GetByEventAsync(eventId, cancellationToken);
        if (quiz is null)
        {
            return new ParticipantQuizScoreSummary(participantId, 0, 0, 0, 1);
        }

        var entries = await BuildLeaderboardAsync(eventId, quiz.Id, cancellationToken);
        var ownEntry = entries.Single(entry => entry.ParticipantId == participantId);
        return new ParticipantQuizScoreSummary(
            participantId,
            ownEntry.TotalScore,
            ownEntry.CorrectCount,
            ownEntry.AnsweredCount,
            ownEntry.Rank);
    }

    public async Task<QuizSessionStatistics> GetSessionStatisticsAsync(
        QuizHostCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureHostAuthorizedAsync(command.EventId, command.HostToken, cancellationToken);
        return await GetPublicSessionStatisticsAsync(
            command.EventId,
            command.SessionId,
            cancellationToken);
    }

    public async Task<QuizSessionStatistics> GetPublicSessionStatisticsAsync(
        Guid eventId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var data = await quizRepository.GetSessionStatisticsAsync(
            eventId,
            sessionId,
            cancellationToken)
            ?? throw new QuizApplicationException(QuizErrorCode.SessionNotFound, "找不到指定的題目場次。");
        var question = await quizRepository.GetQuestionAsync(data.QuestionId, cancellationToken)
            ?? throw new QuizApplicationException(QuizErrorCode.QuestionNotFound, "找不到指定的題目。");
        var counts = data.OptionCounts.ToDictionary(item => item.OptionId, item => item.AnswerCount);
        var incorrectCount = data.AnsweredCount - data.CorrectCount;
        return new QuizSessionStatistics(
            sessionId,
            data.ParticipantCount,
            data.AnsweredCount,
            data.CorrectCount,
            incorrectCount,
            data.ParticipantCount - data.AnsweredCount,
            data.AnsweredCount == 0 ? 0 : decimal.Round((decimal)data.CorrectCount / data.AnsweredCount, 4),
            question.Options
                .OrderBy(option => option.Order)
                .Select(option => new QuizOptionStatistics(
                    option.Id,
                    option.Text,
                    counts.GetValueOrDefault(option.Id)))
                .ToArray());
    }

    public async Task<(int TotalScore, int Rank)> GetParticipantTotalAndRankAsync(
        Guid eventId,
        Guid quizId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        var entries = await BuildLeaderboardAsync(eventId, quizId, cancellationToken);
        var ownEntry = entries.Single(entry => entry.ParticipantId == participantId);
        return (ownEntry.TotalScore, ownEntry.Rank);
    }

    private async Task<IReadOnlyList<QuizLeaderboardEntry>> BuildLeaderboardAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken)
    {
        var rows = await quizRepository.ListLeaderboardRowsAsync(eventId, quizId, cancellationToken);
        return rows
            .OrderByDescending(row => row.TotalScore)
            .ThenByDescending(row => row.CorrectCount)
            .ThenBy(row => row.ParticipantId)
            .Select((row, index) => new QuizLeaderboardEntry(
                index + 1,
                row.ParticipantId,
                row.DisplayName,
                row.Department,
                row.TableNumber,
                row.TotalScore,
                row.CorrectCount,
                row.AnsweredCount))
            .ToArray();
    }

    private async Task AuthorizeViewerAsync(
        Guid eventId,
        string? hostToken,
        Guid? participantId,
        string? participantToken,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(hostToken))
        {
            await EnsureHostAuthorizedAsync(eventId, hostToken, cancellationToken);
            return;
        }

        if (participantId.HasValue && !string.IsNullOrWhiteSpace(participantToken))
        {
            _ = await participantService.ValidateSessionCredentialAsync(
                eventId,
                participantId.Value,
                participantToken,
                cancellationToken);
            return;
        }

        throw new UnauthorizedAccessException("必須提供 Host 或 Participant credential。");
    }

    private async Task EnsureHostAuthorizedAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        if (!await eventService.IsHostAuthorizedAsync(eventId, hostToken, cancellationToken))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }
    }
}
