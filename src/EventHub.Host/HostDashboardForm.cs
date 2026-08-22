using EventHub.Host.Views;
using EventHub.Presentation;

namespace EventHub.Host;

internal enum HostView
{
    Dashboard,
    Event,
    QuestionBank,
    Quiz,
    Results,
    Display
}

public partial class HostDashboardForm : Form
{
    private readonly EventHubHostClient client = new();
    private readonly QrCodePngGenerator qrCodeGenerator = new();
    private readonly Dictionary<Guid, ParticipantView> participants = [];
    private IReadOnlyList<QuestionBankSummaryView> questionBanks = [];
    private IReadOnlyList<QuestionBankQuestionView> questions = [];
    private QuizStateView quizState = CreateWaitingState();
    private Guid? selectedQuestionId;
    private string currentEventName = "尚未選擇";
    private string currentQuizTitle = "尚未選擇";
    private HostConnectionState connectionState = HostConnectionState.Disconnected;
    private DisplayMode displayMode = DisplayMode.Waiting;
    private CancellationTokenSource? deadlineRecoveryCancellation;

    public HostDashboardForm()
    {
        InitializeComponent();
        WireViewEvents();
        WireClientEvents();
        ShowView(HostView.Dashboard);
        RenderContext();
    }

    private void WireViewEvents()
    {
        eventManagementView.CreateEventRequested += createEventRequested;
        eventManagementView.ConnectRequested += connectRequested;
        questionBankView.ImportRequested += importQuestionBankRequested;
        questionBankView.ExportTemplateRequested += exportTemplateRequested;
        questionBankView.RefreshRequested += refreshQuestionBanksRequested;
        questionBankView.SelectedBankChanged += selectedQuestionBankChanged;
        questionBankView.SelectedQuestionChanged += selectedQuestionChanged;
        quizControlView.PrimaryActionRequested += primaryQuizActionRequested;
        quizControlView.ManualQuestionCreateRequested += createManualQuestionRequested;
        displayControlView.DisplayModeRequested += displayModeRequested;
    }

    private void WireClientEvents()
    {
        client.ParticipantChanged += participant => RunOnUi(() => HandleParticipantChanged(participant));
        client.QuizStateRefreshRequested += () => RunOnUiAsync(RefreshQuizStateAsync);
        client.QuestionProgressChanged += progress => RunOnUi(() => HandleQuestionProgress(progress));
        client.LeaderboardRefreshRequested += () => RunOnUiAsync(RefreshResultsAsync);
        client.DisplayModeChanged += mode => RunOnUi(() =>
        {
            displayMode = mode;
            displayControlView.Render(mode);
            RenderContext();
        });
        client.ConnectionStateChanged += state => RunOnUi(() =>
        {
            connectionState = state;
            RenderContext();
        });
        client.RecoveryRequested += () => RunOnUiAsync(RecoverCurrentContextAsync);
    }

    private async void createEventRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                var result = await client.CreateEventAsync(
                    eventManagementView.ServerUrl,
                    eventManagementView.EventName,
                    eventManagementView.EventDateUtc);
                eventManagementView.SetEventCredential(result.Event.Id, result.HostToken);
                currentEventName = result.Event.Name;
                RenderJoinInfo(result.JoinInfo);
                await ConnectAndRecoverAsync();
                eventManagementView.SetStatus($"已建立並連線：{result.Event.Name}");
            });
    }

    private async void connectRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                await ConnectAndRecoverAsync();
                eventManagementView.SetStatus("已連線，正在監看活動。");
            });
    }

    private async Task ConnectAndRecoverAsync()
    {
        await client.ConnectAsync(
            eventManagementView.ServerUrl,
            eventManagementView.EventId,
            eventManagementView.HostToken);
        await RecoverCurrentContextAsync();
    }

    private async Task RecoverCurrentContextAsync()
    {
        var joinInfo = await client.GetJoinInfoAsync();
        currentEventName = joinInfo.EventName;
        RenderJoinInfo(joinInfo);
        var participantItems = await client.GetParticipantsAsync();
        participants.Clear();
        foreach (var participant in participantItems)
        {
            participants[participant.Id] = participant;
        }

        eventManagementView.RenderParticipants(participantItems);
        quizState = await client.GetQuizStateAsync();
        selectedQuestionId = quizState.QuestionId ?? selectedQuestionId;
        var displayState = await client.GetDisplayStateAsync();
        displayMode = displayState.Mode;
        displayControlView.Render(displayMode);
        await LoadQuestionBanksAsync(selectedQuestionId: selectedQuestionId);
        RenderQuizState();
        if (quizState.State == QuizState.Revealed)
        {
            await RefreshResultsAsync();
        }

        RenderContext();
    }

    private void RenderJoinInfo(EventJoinInfoView joinInfo)
    {
        var png = qrCodeGenerator.Generate(joinInfo.JoinUrl);
        using var stream = new MemoryStream(png);
        using var source = Image.FromStream(stream);
        eventManagementView.RenderJoinInfo(joinInfo, new Bitmap(source));
    }

    private async void importQuestionBankRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            questionBankView.SetStatus,
            questionBankView.SetBusy,
            async () =>
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "CSV 題庫 (*.csv)|*.csv",
                    Title = "選擇 EventHub 題庫 CSV",
                    CheckFileExists = true
                };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    questionBankView.SetStatus("已取消題庫匯入。");
                    return;
                }

                var preview = await client.PreviewQuestionBankAsync(dialog.FileName);
                using var previewForm = new QuestionBankImportPreviewForm(preview);
                if (previewForm.ShowDialog(this) != DialogResult.OK)
                {
                    questionBankView.SetStatus(
                        preview.IsValid ? "已取消題庫匯入。" : "CSV 驗證失敗，未匯入任何資料。",
                        !preview.IsValid);
                    return;
                }

                var result = await client.ImportQuestionBankAsync(dialog.FileName);
                await LoadQuestionBanksAsync(result.QuizId);
                questionBankView.SetStatus($"已匯入：{result.QuizTitle}（{result.QuestionCount} 題）");
            });
    }

    private async void exportTemplateRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            questionBankView.SetStatus,
            questionBankView.SetBusy,
            async () =>
            {
                using var dialog = new SaveFileDialog
                {
                    Filter = "CSV 檔案 (*.csv)|*.csv",
                    FileName = "EventHub_QuizTemplate.csv",
                    Title = "儲存 EventHub 題庫範本"
                };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                await File.WriteAllBytesAsync(
                    dialog.FileName,
                    await client.DownloadQuestionBankTemplateAsync());
                questionBankView.SetStatus($"已匯出範本：{dialog.FileName}");
            });
    }

    private async void refreshQuestionBanksRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            questionBankView.SetStatus,
            questionBankView.SetBusy,
            async () =>
            {
                await LoadQuestionBanksAsync(questionBankView.SelectedBank?.Id, selectedQuestionId);
                questionBankView.SetStatus("題庫已重新整理。");
            });
    }

    private async void selectedQuestionBankChanged(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            questionBankView.SetStatus,
            questionBankView.SetBusy,
            async () =>
            {
                await LoadSelectedBankQuestionsAsync(selectedQuestionId);
                questionBankView.SetStatus("已切換題庫。");
            });
    }

    private void selectedQuestionChanged(object? sender, EventArgs e)
    {
        var selected = questionBankView.SelectedQuestion;
        if (selected is null)
        {
            return;
        }

        selectedQuestionId = selected.Id;
        currentQuizTitle = questionBankView.SelectedBank?.Title ?? currentQuizTitle;
        RenderQuizState();
        RenderContext();
    }

    private async Task LoadQuestionBanksAsync(Guid? selectedQuizId = null, Guid? selectedQuestionId = null)
    {
        questionBanks = await client.GetQuestionBanksAsync();
        var targetQuizId = selectedQuizId ?? questionBankView.SelectedBank?.Id;
        questionBankView.RenderBanks(questionBanks, targetQuizId);
        await LoadSelectedBankQuestionsAsync(selectedQuestionId);
    }

    private async Task LoadSelectedBankQuestionsAsync(Guid? targetQuestionId)
    {
        var selectedBank = questionBankView.SelectedBank;
        if (selectedBank is null)
        {
            questions = [];
            currentQuizTitle = "尚未選擇";
            RenderContext();
            return;
        }

        questions = await client.GetQuestionsAsync(selectedBank.Id);
        currentQuizTitle = selectedBank.Title;
        questionBankView.RenderQuestions(questions, targetQuestionId ?? selectedQuestionId);
        selectedQuestionId = questionBankView.SelectedQuestion?.Id ?? selectedQuestionId;
        RenderQuizState();
        RenderContext();
    }

    private async void primaryQuizActionRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            quizControlView.SetStatus,
            quizControlView.SetBusy,
            async () =>
            {
                switch (quizControlView.CurrentAction)
                {
                    case QuizPrimaryAction.Start:
                        if (!selectedQuestionId.HasValue)
                        {
                            throw new InvalidOperationException("請先到題庫選擇題目。");
                        }

                        quizState = await client.StartQuestionAsync(selectedQuestionId.Value);
                        ShowView(HostView.Quiz);
                        break;
                    case QuizPrimaryAction.Close:
                        quizState = await client.CloseQuestionAsync(GetCurrentSessionId());
                        break;
                    case QuizPrimaryAction.Reveal:
                        await client.RevealAnswerAsync(GetCurrentSessionId());
                        quizState = await client.GetQuizStateAsync();
                        await RefreshResultsAsync();
                        break;
                    case QuizPrimaryAction.Next:
                        if (!questionBankView.MoveSelection(1))
                        {
                            throw new InvalidOperationException("題庫中沒有下一題。");
                        }

                        break;
                    default:
                        throw new InvalidOperationException("目前沒有可執行的 Quiz 操作。");
                }

                await RefreshQuestionStatusesAsync();
                RenderQuizState();
                quizControlView.SetStatus("操作完成。");
            });
    }

    private async void createManualQuestionRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            quizControlView.SetStatus,
            quizControlView.SetBusy,
            async () =>
            {
                var created = await client.CreateQuestionAsync(quizControlView.BuildManualQuestionRequest());
                await SelectQuestionAcrossBanksAsync(created.Id);
                quizControlView.SetStatus($"已建立手動題目：{created.Text}");
            });
    }

    private async Task SelectQuestionAcrossBanksAsync(Guid questionId)
    {
        questionBanks = await client.GetQuestionBanksAsync();
        foreach (var bank in questionBanks)
        {
            var bankQuestions = await client.GetQuestionsAsync(bank.Id);
            if (bankQuestions.Any(question => question.Id == questionId))
            {
                questionBankView.RenderBanks(questionBanks, bank.Id);
                questions = bankQuestions;
                questionBankView.RenderQuestions(bankQuestions, questionId);
                selectedQuestionId = questionId;
                currentQuizTitle = bank.Title;
                RenderQuizState();
                RenderContext();
                return;
            }
        }

        throw new InvalidOperationException("題目已建立，但重新載入題庫時找不到該題目。");
    }

    private async Task RefreshQuizStateAsync()
    {
        quizState = await client.GetQuizStateAsync();
        selectedQuestionId = quizState.QuestionId ?? selectedQuestionId;
        await RefreshQuestionStatusesAsync();
        RenderQuizState();
        if (quizState.State == QuizState.Revealed)
        {
            await RefreshResultsAsync();
        }
    }

    private async Task RefreshQuestionStatusesAsync()
    {
        if (questionBankView.SelectedBank is not null)
        {
            await LoadSelectedBankQuestionsAsync(selectedQuestionId);
        }
    }

    private void RenderQuizState()
    {
        var selected = questionBankView.SelectedQuestion;
        var position = selected is null || questions.Count == 0
            ? "— / —"
            : $"{GetQuestionIndex(selected) + 1:00} / {questions.Count:00}";
        var stateForPresentation = quizState;
        if (quizState.State == QuizState.Revealed && selected?.Id != quizState.QuestionId)
        {
            stateForPresentation = quizState with
            {
                State = QuizState.Waiting,
                SessionId = null,
                QuestionId = null,
                QuestionText = null,
                Options = [],
                StartedAtUtc = null,
                AnswerDeadlineUtc = null,
                AnsweredCount = 0,
                CorrectOptionId = null
            };
        }

        quizControlView.Render(
            currentQuizTitle,
            position,
            selected,
            stateForPresentation,
            questionBankView.HasNextQuestion);
        var canNavigate = quizState.State is QuizState.Waiting or QuizState.Revealed;
        questionBankView.SetNavigationEnabled(canNavigate);
        ScheduleDeadlineRecovery(quizState);
        RenderContext();
    }

    private void ScheduleDeadlineRecovery(QuizStateView state)
    {
        deadlineRecoveryCancellation?.Cancel();
        deadlineRecoveryCancellation?.Dispose();
        deadlineRecoveryCancellation = null;
        if (state.State != QuizState.Open || !state.AnswerDeadlineUtc.HasValue)
        {
            return;
        }

        deadlineRecoveryCancellation = new CancellationTokenSource();
        _ = RefreshAtDeadlineAsync(state.AnswerDeadlineUtc.Value, deadlineRecoveryCancellation.Token);
    }

    private async Task RefreshAtDeadlineAsync(DateTimeOffset deadlineUtc, CancellationToken cancellationToken)
    {
        try
        {
            var delay = deadlineUtc - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                RunOnUiAsync(RefreshQuizStateAsync);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RefreshResultsAsync()
    {
        if (!quizState.SessionId.HasValue || quizState.State != QuizState.Revealed)
        {
            quizResultView.Clear();
            return;
        }

        var statistics = await client.GetStatisticsAsync(quizState.SessionId.Value);
        var leaderboard = await client.GetLeaderboardAsync();
        quizResultView.Render(statistics, leaderboard, quizState.CorrectOptionId);
    }

    private async void displayModeRequested(DisplayMode mode)
    {
        await RunViewOperationAsync(
            displayControlView.SetStatus,
            displayControlView.SetBusy,
            async () =>
            {
                var state = await client.SetDisplayModeAsync(mode);
                displayMode = state.Mode;
                displayControlView.Render(displayMode);
                displayControlView.SetStatus($"大螢幕已切換至 {displayMode}。");
                RenderContext();
            });
    }

    private void HandleParticipantChanged(ParticipantView participant)
    {
        participants[participant.Id] = participant;
        eventManagementView.UpsertParticipant(participant);
        RenderContext();
    }

    private void HandleQuestionProgress(QuestionProgressNotification progress)
    {
        if (quizState.SessionId != progress.SessionId)
        {
            return;
        }

        quizState = quizState with
        {
            AnsweredCount = progress.AnsweredCount,
            ParticipantCount = progress.ParticipantCount,
            OnlineCount = progress.OnlineCount
        };
        quizControlView.UpdateProgress(progress);
        RenderContext();
    }

    private void RenderContext()
    {
        var selected = questionBankView.SelectedQuestion;
        var position = selected is null || questions.Count == 0
            ? "— / —"
            : $"{GetQuestionIndex(selected) + 1} / {questions.Count}";
        currentEventHeaderLabel.Text = $"Current Event：{currentEventName}";
        currentQuizHeaderLabel.Text = $"Quiz：{currentQuizTitle}";
        participantHeaderLabel.Text = $"Participants {participants.Count}";
        serverHeaderLabel.Text = connectionState switch
        {
            HostConnectionState.Connected => "● Server Connected",
            HostConnectionState.Connecting => "● Server Connecting...",
            HostConnectionState.Reconnecting => "● Server Reconnecting...",
            _ => "● Server Disconnected"
        };
        serverHeaderLabel.ForeColor = connectionState == HostConnectionState.Connected
            ? Color.DarkGreen
            : connectionState == HostConnectionState.Disconnected
                ? Color.Firebrick
                : Color.DarkOrange;
        displayHeaderLabel.Text = $"Display: {displayMode}";
        dashboardView.Render(
            currentEventName,
            currentQuizTitle,
            participants.Count,
            position,
            quizState.State,
            connectionState,
            displayMode);
    }

    private void navigationButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button { Tag: HostView view })
        {
            ShowView(view);
        }
    }

    private void ShowView(HostView view)
    {
        dashboardView.Visible = view == HostView.Dashboard;
        eventManagementView.Visible = view == HostView.Event;
        questionBankView.Visible = view == HostView.QuestionBank;
        quizControlView.Visible = view == HostView.Quiz;
        quizResultView.Visible = view == HostView.Results;
        displayControlView.Visible = view == HostView.Display;
        var selectedControl = view switch
        {
            HostView.Dashboard => (Control)dashboardView,
            HostView.Event => eventManagementView,
            HostView.QuestionBank => questionBankView,
            HostView.Quiz => quizControlView,
            HostView.Results => quizResultView,
            HostView.Display => displayControlView,
            _ => dashboardView
        };
        selectedControl.BringToFront();
        foreach (var button in navigationPanel.Controls.OfType<Button>())
        {
            button.BackColor = button.Tag is HostView buttonView && buttonView == view
                ? Color.FromArgb(35, 101, 150)
                : Color.FromArgb(22, 43, 65);
        }
    }

    private async Task RunViewOperationAsync(
        Action<string, bool> setStatus,
        Action<bool> setBusy,
        Func<Task> operation)
    {
        setBusy(true);
        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            setStatus(exception.Message, true);
        }
        finally
        {
            setBusy(false);
            RenderQuizState();
        }
    }

    private Guid GetCurrentSessionId() => quizState.SessionId
        ?? throw new InvalidOperationException("目前沒有題目場次。");

    private int GetQuestionIndex(QuestionBankQuestionView question)
    {
        for (var index = 0; index < questions.Count; index++)
        {
            if (questions[index].Id == question.Id)
            {
                return index;
            }
        }

        return -1;
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }

    private void RunOnUiAsync(Func<Task> action)
    {
        RunOnUi(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                quizControlView.SetStatus($"狀態同步失敗：{exception.Message}", true);
            }
        });
    }

    private async void HostDashboardForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        deadlineRecoveryCancellation?.Cancel();
        deadlineRecoveryCancellation?.Dispose();
        await client.DisposeAsync();
    }

    private static QuizStateView CreateWaitingState() =>
        new(QuizState.Waiting, null, null, null, [], null, null, 0, 0, 0, null);
}
