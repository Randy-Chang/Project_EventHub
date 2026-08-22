using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public sealed class QuizService(
    IQuizRepository quizRepository,
    IParticipantRepository participantRepository,
    IParticipantPresenceStore presenceStore,
    EventService eventService,
    ParticipantService participantService,
    QuizScoringService scoringService,
    TimeProvider timeProvider)
{
    public async Task<QuizQuestionSummary> CreateQuestionAsync(
        CreateQuizQuestionCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureHostAuthorizedAsync(command.EventId, command.HostToken, cancellationToken);
        _ = await eventService.GetAsync(command.EventId, cancellationToken);

        var quiz = await quizRepository.GetByEventAsync(command.EventId, cancellationToken);
        if (quiz is null)
        {
            quiz = Quiz.Create(command.EventId, "快問快答", timeProvider.GetUtcNow());
            await quizRepository.AddQuizAsync(quiz, cancellationToken);
        }

        var order = await quizRepository.GetNextQuestionOrderAsync(quiz.Id, cancellationToken);
        var question = QuizQuestion.Create(
            quiz.Id,
            command.QuestionText,
            command.Options,
            command.CorrectOptionIndex,
            TimeSpan.FromSeconds(command.AnswerDurationSeconds),
            order);

        await quizRepository.AddQuestionAsync(question, cancellationToken);
        return ToQuestionSummary(question);
    }

    public async Task<CurrentQuizState> StartQuestionAsync(
        StartQuizQuestionCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureHostAuthorizedAsync(command.EventId, command.HostToken, cancellationToken);
        var question = await GetRequiredQuestionAsync(command.QuestionId, cancellationToken);
        var quiz = await quizRepository.GetByEventAsync(command.EventId, cancellationToken);
        if (quiz is null || question.QuizId != quiz.Id)
        {
            throw new QuizApplicationException(QuizErrorCode.QuestionNotFound, "找不到此活動的指定題目。");
        }

        var current = await quizRepository.GetCurrentSessionAsync(command.EventId, cancellationToken);
        if (current?.State == QuizQuestionState.Open)
        {
            var now = timeProvider.GetUtcNow();
            if (!current.CloseIfDeadlinePassed(now))
            {
                throw new QuizApplicationException(QuizErrorCode.QuestionAlreadyOpen, "已有題目正在開放作答。");
            }

            await quizRepository.UpdateSessionAsync(current, cancellationToken);
        }

        var session = QuizQuestionSession.Create(command.EventId, quiz.Id, question.Id);
        session.Open(timeProvider.GetUtcNow(), question.AnswerDuration);
        await quizRepository.AddSessionAsync(session, cancellationToken);
        return await BuildStateAsync(session, question, null, cancellationToken);
    }

    public async Task<SubmitQuizAnswerResult> SubmitAnswerAsync(
        SubmitQuizAnswerCommand command,
        CancellationToken cancellationToken)
    {
        _ = await participantService.ValidateSessionCredentialAsync(
            command.EventId,
            command.ParticipantId,
            command.SessionToken,
            cancellationToken);

        var session = await GetRequiredSessionAsync(command.EventId, command.SessionId, cancellationToken);
        var question = await GetRequiredQuestionAsync(session.QuestionId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (session.CloseIfDeadlinePassed(now))
        {
            await quizRepository.UpdateSessionAsync(session, cancellationToken);
            return new SubmitQuizAnswerResult(
                SubmitQuizAnswerStatus.DeadlinePassed,
                null,
                await GetProgressAsync(command.EventId, session.Id, cancellationToken));
        }

        if (session.State != QuizQuestionState.Open)
        {
            throw new QuizApplicationException(QuizErrorCode.InvalidQuestionState, "題目目前未開放作答。");
        }

        if (!question.ContainsOption(command.SelectedOptionId))
        {
            throw new QuizApplicationException(QuizErrorCode.InvalidOption, "所選答案不屬於目前題目。");
        }

        var existing = await quizRepository.GetAnswerAsync(session.Id, command.ParticipantId, cancellationToken);
        if (existing is not null)
        {
            return new SubmitQuizAnswerResult(
                SubmitQuizAnswerStatus.AlreadyAnswered,
                existing.SubmittedAtUtc,
                await GetProgressAsync(command.EventId, session.Id, cancellationToken));
        }

        var answer = ParticipantAnswer.Create(
            command.EventId,
            session.Id,
            command.ParticipantId,
            command.SelectedOptionId,
            now);

        var wasAdded = await quizRepository.TryAddAnswerAsync(answer, cancellationToken);
        return new SubmitQuizAnswerResult(
            wasAdded ? SubmitQuizAnswerStatus.Accepted : SubmitQuizAnswerStatus.AlreadyAnswered,
            now,
            await GetProgressAsync(command.EventId, session.Id, cancellationToken));
    }

    public async Task<CurrentQuizState> CloseQuestionAsync(
        QuizHostCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureHostAuthorizedAsync(command.EventId, command.HostToken, cancellationToken);
        var session = await GetRequiredSessionAsync(command.EventId, command.SessionId, cancellationToken);
        session.Close(timeProvider.GetUtcNow());
        await quizRepository.UpdateSessionAsync(session, cancellationToken);
        var question = await GetRequiredQuestionAsync(session.QuestionId, cancellationToken);
        return await BuildStateAsync(session, question, null, cancellationToken);
    }

    public Task<RevealQuizQuestionResult> RevealAnswerAsync(
        QuizHostCommand command,
        CancellationToken cancellationToken) =>
        scoringService.RevealAnswerAsync(command, cancellationToken);

    public async Task<CurrentQuizState> GetCurrentStateAsync(
        QuizStateQuery query,
        CancellationToken cancellationToken)
    {
        Guid? participantId = null;
        if (!string.IsNullOrWhiteSpace(query.HostToken))
        {
            await EnsureHostAuthorizedAsync(query.EventId, query.HostToken, cancellationToken);
        }
        else if (query.ParticipantId.HasValue && !string.IsNullOrWhiteSpace(query.SessionToken))
        {
            _ = await participantService.ValidateSessionCredentialAsync(
                query.EventId,
                query.ParticipantId.Value,
                query.SessionToken,
                cancellationToken);
            participantId = query.ParticipantId;
        }
        else
        {
            throw new UnauthorizedAccessException("必須提供 Host 或 Participant credential。");
        }

        var session = await quizRepository.GetCurrentSessionAsync(query.EventId, cancellationToken);
        if (session is null)
        {
            return await BuildWaitingStateAsync(query.EventId, cancellationToken);
        }

        if (session.CloseIfDeadlinePassed(timeProvider.GetUtcNow()))
        {
            await quizRepository.UpdateSessionAsync(session, cancellationToken);
        }

        var question = await GetRequiredQuestionAsync(session.QuestionId, cancellationToken);
        return await BuildStateAsync(session, question, participantId, cancellationToken);
    }

    private async Task<CurrentQuizState> BuildStateAsync(
        QuizQuestionSession session,
        QuizQuestion question,
        Guid? participantId,
        CancellationToken cancellationToken)
    {
        ParticipantAnswer? answer = null;
        if (participantId.HasValue)
        {
            answer = await quizRepository.GetAnswerAsync(session.Id, participantId.Value, cancellationToken);
        }

        var progress = await GetProgressAsync(session.EventId, session.Id, cancellationToken);
        var isRevealed = session.State == QuizQuestionState.Revealed;
        ParticipantQuestionResult? questionResult = null;
        (int TotalScore, int Rank)? totalAndRank = null;
        if (isRevealed && participantId.HasValue)
        {
            questionResult = await quizRepository.GetQuestionResultAsync(
                session.Id,
                participantId.Value,
                cancellationToken);
            totalAndRank = await scoringService.GetParticipantTotalAndRankAsync(
                session.EventId,
                session.QuizId,
                participantId.Value,
                cancellationToken);
        }

        return new CurrentQuizState(
            session.State,
            session.Id,
            question.Id,
            question.Text,
            question.Options.OrderBy(option => option.Order).Select(ToOptionSummary).ToArray(),
            session.StartedAtUtc,
            session.AnswerDeadlineUtc,
            progress.AnsweredCount,
            progress.ParticipantCount,
            progress.OnlineCount,
            answer?.SelectedOptionId,
            answer is not null,
            isRevealed ? question.CorrectOptionId : null,
            isRevealed && answer is not null ? answer.SelectedOptionId == question.CorrectOptionId : null,
            questionResult?.BaseScore,
            questionResult?.SpeedBonus,
            questionResult?.Score,
            totalAndRank?.TotalScore,
            totalAndRank?.Rank);
    }

    private async Task<CurrentQuizState> BuildWaitingStateAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var progress = await GetProgressAsync(eventId, null, cancellationToken);
        return new CurrentQuizState(
            QuizQuestionState.Waiting,
            null,
            null,
            null,
            [],
            null,
            null,
            0,
            progress.ParticipantCount,
            progress.OnlineCount,
            null,
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private async Task<QuizProgress> GetProgressAsync(
        Guid eventId,
        Guid? sessionId,
        CancellationToken cancellationToken)
    {
        var answeredCount = sessionId.HasValue
            ? await quizRepository.CountAnswersAsync(sessionId.Value, cancellationToken)
            : 0;
        var participantCount = await participantRepository.CountByEventAsync(eventId, cancellationToken);
        var onlineCount = presenceStore.GetOnlineParticipantIds(eventId).Count;
        return new QuizProgress(answeredCount, participantCount, onlineCount);
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

    private async Task<QuizQuestion> GetRequiredQuestionAsync(Guid questionId, CancellationToken cancellationToken) =>
        await quizRepository.GetQuestionAsync(questionId, cancellationToken)
        ?? throw new QuizApplicationException(QuizErrorCode.QuestionNotFound, "找不到指定的題目。");

    private async Task<QuizQuestionSession> GetRequiredSessionAsync(
        Guid eventId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var session = await quizRepository.GetCurrentSessionAsync(eventId, cancellationToken);
        if (session is null || session.Id != sessionId)
        {
            throw new QuizApplicationException(QuizErrorCode.SessionNotFound, "找不到指定的題目場次。");
        }

        return session;
    }

    private static QuizQuestionSummary ToQuestionSummary(QuizQuestion question) =>
        new(
            question.Id,
            question.Text,
            question.Options.OrderBy(option => option.Order).Select(ToOptionSummary).ToArray(),
            (int)question.AnswerDuration.TotalSeconds,
            question.Order);

    private static QuizOptionSummary ToOptionSummary(QuizOption option) => new(option.Id, option.Text, option.Order);
}
