using EventHub.Host.Views;
using EventHub.Presentation;

namespace EventHub.Host;

internal enum HostView
{
    Dashboard,
    Event,
    QuestionBank,
    Quiz
}

public partial class HostDashboardForm : Form
{
    private readonly EventHubHostClient client = new();
    private readonly QrCodePngGenerator qrCodeGenerator = new();
    private readonly NetworkInterfaceDiscovery networkInterfaceDiscovery = new();
    private readonly FirewallRuleService firewallRuleService = new();
    private readonly HostSessionStore hostSessionStore = new();
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
    private QuizPrimaryAction currentPrimaryAction;
    private RecentHostSession? recentSession;
    private EventState eventState = EventState.Draft;
    private bool isJoinOpen;
    private HostRecommendedAction recommendedAction = HostRecommendedAction.PrepareEvent;

    public HostDashboardForm()
    {
        InitializeComponent();
        WireViewEvents();
        WireClientEvents();
        RefreshLanAddresses();
        ShowView(HostView.Dashboard);
        RenderQuizState();
        Shown += HostDashboardForm_Shown;
    }

    private void WireViewEvents()
    {
        eventManagementView.CreateEventRequested += createEventRequested;
        eventManagementView.ConnectRequested += connectRequested;
        eventManagementView.ResumeRecentRequested += resumeRecentRequested;
        eventManagementView.ForgetRecentRequested += forgetRecentRequested;
        eventManagementView.ToggleJoinPolicyRequested += toggleJoinPolicyRequested;
        eventManagementView.RefreshLanAddressesRequested += refreshLanAddressesRequested;
        eventManagementView.TestConnectionRequested += testConnectionRequested;
        eventManagementView.InstallFirewallRuleRequested += installFirewallRuleRequested;
        questionBankView.ImportRequested += importQuestionBankRequested;
        questionBankView.ExportTemplateRequested += exportTemplateRequested;
        questionBankView.RefreshRequested += refreshQuestionBanksRequested;
        questionBankView.SelectedBankChanged += selectedQuestionBankChanged;
        questionBankView.SelectedQuestionChanged += selectedQuestionChanged;
        quizActivityView.SelectedQuestionChanged += quizActivitySelectedQuestionChanged;
        quizActivityView.DefaultPracticeRequested += defaultPracticeRequested;
        liveMonitorView.DisplayModeRequested += displayModeRequested;
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
            liveMonitorView.RenderDisplayMode(mode);
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
                await PrepareNetworkAsync();
                ResetCurrentEventPresentation();
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
                await PrepareNetworkAsync();
                ResetCurrentEventPresentation();
                await ConnectAndRecoverAsync();
                eventManagementView.SetStatus("已連線，正在監看活動。");
            });
    }

    private void refreshLanAddressesRequested(object? sender, EventArgs e)
    {
        RefreshLanAddresses();
        eventManagementView.SetStatus("已重新掃描 LAN 網卡，請確認選取的 IPv4。", OperationMessageKind.Information);
    }

    private async void testConnectionRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                var health = await PrepareNetworkAsync();
                eventManagementView.SetStatus(
                    $"連線檢查成功：{health.RequestBaseUrl}（Server {health.Version ?? "unknown"}）。手機請使用 {eventManagementView.PublicServerUrl}。",
                    OperationMessageKind.Success);
                if (connectionState == HostConnectionState.Connected)
                {
                    await RecoverCurrentContextAsync();
                }
            });
    }

    private async void installFirewallRuleRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                await firewallRuleService.EnsureInboundRuleAsync(eventManagementView.PublicServerUrl);
                eventManagementView.RenderFirewallReady();
                eventManagementView.SetStatus(
                    "Windows Firewall Private profile inbound rule 已就緒，請再執行連線檢查。",
                    OperationMessageKind.Success);
            });
    }

    private void RefreshLanAddresses() =>
        eventManagementView.RenderLanAddresses(networkInterfaceDiscovery.GetAvailableAddresses());

    private async Task<ServerHealthView> PrepareNetworkAsync()
    {
        var publicServerUrl = eventManagementView.PublicServerUrl;
        var health = await client.CheckHealthAsync(publicServerUrl);
        _ = await client.ConfigurePublicBaseUrlAsync(eventManagementView.ServerUrl, publicServerUrl);
        eventManagementView.RenderNetworkReady();
        return health;
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
        var snapshot = await client.GetHostSessionAsync();
        currentEventName = snapshot.Event.Name;
        eventState = snapshot.Event.State;
        isJoinOpen = snapshot.Event.IsJoinOpen;
        recommendedAction = snapshot.RecommendedAction;
        RenderJoinInfo(snapshot.JoinInfo);
        eventManagementView.SetJoinPolicyAvailability(eventState is EventState.Ready or EventState.Active);
        var participantItems = snapshot.Participants;
        participants.Clear();
        foreach (var participant in participantItems)
        {
            participants[participant.Id] = participant;
        }

        eventManagementView.RenderParticipants(participantItems);
        quizState = snapshot.Quiz;
        selectedQuestionId = quizState.QuestionId ?? selectedQuestionId;
        displayMode = snapshot.Display.Mode;
        liveMonitorView.RenderDisplayMode(displayMode);
        questionBanks = snapshot.QuestionBanks;
        questionBankView.RenderBanks(questionBanks, questionBankView.SelectedBank?.Id);
        await LoadSelectedBankQuestionsAsync(selectedQuestionId);
        RenderQuizState();
        if (quizState.State == QuizState.Revealed)
        {
            await RefreshResultsAsync();
        }

        RenderContext();
        recentSession = new RecentHostSession(
            eventManagementView.ServerUrl,
            snapshot.Event.Id,
            snapshot.Event.Name,
            eventManagementView.HostToken,
            DateTimeOffset.UtcNow);
        await hostSessionStore.SaveAsync(recentSession);
        eventManagementView.RenderRecentSession(recentSession);
        if (snapshot.WasDeadlineRecovered)
        {
            eventManagementView.SetStatus(
                "活動中斷期間題目已截止，Server 已自動關閉作答。",
                OperationMessageKind.Warning);
        }
    }

    private async void HostDashboardForm_Shown(object? sender, EventArgs e)
    {
        recentSession = await hostSessionStore.TryLoadAsync();
        eventManagementView.RenderRecentSession(recentSession);
        ShowView(HostView.Event);
    }

    private async void resumeRecentRequested(object? sender, EventArgs e)
    {
        if (recentSession is null)
        {
            return;
        }

        eventManagementView.SetResumeContext(recentSession);
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                await PrepareNetworkAsync();
                await ConnectAndRecoverAsync();
                var deadlineMessage = quizState.State == QuizState.Closed
                    ? "題目已關閉，等待公布答案。"
                    : $"題目狀態：{quizState.State}";
                MessageBox.Show(
                    this,
                    $"活動已恢復\r\n\r\n活動：{currentEventName}\r\n活動狀態：{eventState}\r\n{deadlineMessage}\r\n已作答：{quizState.AnsweredCount} / {quizState.ParticipantCount}\r\n在線：{quizState.OnlineCount}\r\n投影幕：{displayMode}\r\n\r\n所有作答與分數均已從 Server 恢復。",
                    "繼續主持",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ShowView(HostView.Dashboard);
            });
    }

    private async void forgetRecentRequested(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            this,
            "這只會清除本機保存的連線資訊，不會刪除 Server 上的活動、答案或分數。\r\n\r\n要從這台電腦移除此活動嗎？",
            "從這台電腦移除",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning);
        if (result != DialogResult.OK)
        {
            return;
        }

        try
        {
            await hostSessionStore.ForgetAsync();
            recentSession = null;
            eventManagementView.RenderRecentSession(null);
            eventManagementView.SetStatus("已從這台電腦移除最近活動連線資訊。", OperationMessageKind.Information);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            eventManagementView.SetStatus($"無法移除本機連線資訊：{exception.Message}", true);
        }
    }

    private async void toggleJoinPolicyRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            eventManagementView.SetStatus,
            eventManagementView.SetBusy,
            async () =>
            {
                _ = await client.ChangeJoinPolicyAsync(!isJoinOpen);
                await RecoverCurrentContextAsync();
                eventManagementView.SetStatus(isJoinOpen ? "已開放參與者報到。" : "已關閉新參與者報到。");
            });
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
                    await client.DownloadQuestionBankTemplateAsync(eventManagementView.ServerUrl));
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

    private async void defaultPracticeRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            SetActionStatus,
            SetPrimaryActionBusy,
            async () =>
            {
                var result = await client.EnsureDefaultPracticeAsync();
                await LoadQuestionBanksAsync(result.QuizId);
                ShowView(HostView.Quiz);
                SetActionStatus("內建熱身已就緒，請按「開始本題」。");
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
        quizActivityView.RenderQuestions(questions, selectedQuestionId);
        RenderQuizState();
        RenderContext();
    }

    private void quizActivitySelectedQuestionChanged(object? sender, EventArgs e)
    {
        var selected = quizActivityView.SelectedQuestion;
        if (selected is null)
        {
            return;
        }

        selectedQuestionId = selected.Id;
        RenderQuizState();
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
            quizActivityView.RenderQuestions([], null);
            RenderContext();
            return;
        }

        questions = await client.GetQuestionsAsync(selectedBank.Id);
        currentQuizTitle = selectedBank.Title;
        questionBankView.RenderQuestions(questions, targetQuestionId ?? selectedQuestionId);
        selectedQuestionId = questionBankView.SelectedQuestion?.Id ?? selectedQuestionId;
        quizActivityView.RenderQuestions(questions, selectedQuestionId);
        RenderQuizState();
        RenderContext();
    }

    private async void primaryQuizActionRequested(object? sender, EventArgs e)
    {
        await RunViewOperationAsync(
            SetActionStatus,
            SetPrimaryActionBusy,
            async () =>
            {
                if (recommendedAction is HostRecommendedAction.PrepareEvent or
                    HostRecommendedAction.OpenJoining or
                    HostRecommendedAction.StartEvent or
                    HostRecommendedAction.CompleteEvent)
                {
                    await ExecuteLifecycleActionAsync();
                    return;
                }

                switch (currentPrimaryAction)
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
                        if (!quizState.Mode.HasValue || !quizActivityView.MoveToNextInMode(quizState.Mode.Value))
                        {
                            throw new InvalidOperationException("此模式沒有下一題。");
                        }
                        selectedQuestionId = quizActivityView.SelectedQuestion?.Id;
                        quizActivityView.ShowQuestion();
                        break;
                    case QuizPrimaryAction.StartOfficial:
                        if (!quizActivityView.SelectFirst(QuizQuestionMode.Scored))
                        {
                            throw new InvalidOperationException("題庫中沒有正式題。");
                        }
                        selectedQuestionId = quizActivityView.SelectedQuestion?.Id;
                        quizState = await client.StartQuestionAsync(selectedQuestionId!.Value);
                        quizActivityView.ShowQuestion();
                        break;
                    case QuizPrimaryAction.FinalLeaderboard:
                        await SetDisplayModeAsync(DisplayMode.Leaderboard);
                        quizActivityView.ShowFinalLeaderboard();
                        break;
                    default:
                        throw new InvalidOperationException("目前沒有可執行的 Quiz 操作。");
                }

                await RefreshQuestionStatusesAsync();
                RenderQuizState();
                SetActionStatus("操作完成。");
            });
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
        var selected = quizActivityView.SelectedQuestion;
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
                CorrectOptionId = null,
                Explanation = null
            };
        }

        quizActivityView.RenderQuestion(currentQuizTitle, stateForPresentation);
        var mode = stateForPresentation.Mode ?? selected?.Mode;
        var hasNext = mode.HasValue && quizActivityView.HasNextInMode(mode.Value);
        var presentation = QuizActionPresentation.Resolve(
            stateForPresentation.State,
            selected is not null || stateForPresentation.QuestionId.HasValue,
            mode,
            hasNext,
            quizActivityView.HasScoredQuestions);
        currentPrimaryAction = presentation.Action;
        var lifecyclePresentation = ResolveLifecyclePresentation();
        primaryActionButton.Text = lifecyclePresentation?.ButtonText ?? presentation.ButtonText;
        primaryActionButton.Enabled = connectionState == HostConnectionState.Connected &&
            (lifecyclePresentation?.IsEnabled ?? presentation.IsEnabled);
        actionGuidanceLabel.Text = lifecyclePresentation?.Guidance ?? presentation.Guidance;
        actionContextLabel.Text = selected is null
            ? "尚未選擇題庫"
            : $"{(selected.Mode == QuizQuestionMode.Practice ? "PRACTICE" : "SCORED")}｜{selected.Text}";
        quizActivityView.SetDefaultPracticeAvailability(
            !quizActivityView.HasPracticeQuestions,
            connectionState == HostConnectionState.Connected && quizState.State == QuizState.Waiting);
        var canNavigate = quizState.State is QuizState.Waiting or QuizState.Revealed;
        questionBankView.SetNavigationEnabled(canNavigate);
        quizActivityView.SetNavigationEnabled(canNavigate);
        liveMonitorView.Render(mode, GetQuestionPosition(selected), stateForPresentation, displayMode);
        ScheduleDeadlineRecovery(quizState);
        RenderContext();
    }

    private (string ButtonText, string Guidance, bool IsEnabled)? ResolveLifecyclePresentation()
    {
        return recommendedAction switch
        {
            HostRecommendedAction.PrepareEvent => ("完成準備", "確認題庫與現場網路後，將活動標記為就緒。", true),
            HostRecommendedAction.OpenJoining => ("開放參與者加入", "開放 QR Code 報到，手機將可加入活動。", true),
            HostRecommendedAction.StartEvent => ("開始正式活動", "開始後才能開放題目作答。", true),
            HostRecommendedAction.CompleteEvent => ("完成活動", "完成後將關閉加入，且無法再開始新題目。", true),
            HostRecommendedAction.ViewCompletedEvent => ("活動已完成", "目前僅可查看已保存的結果。", false),
            _ => null
        };
    }

    private async Task ExecuteLifecycleActionAsync()
    {
        if (recommendedAction == HostRecommendedAction.PrepareEvent)
        {
            _ = await client.ChangeEventStateAsync(EventState.Ready);
        }
        else if (recommendedAction == HostRecommendedAction.OpenJoining)
        {
            _ = await client.ChangeJoinPolicyAsync(true);
        }
        else if (recommendedAction == HostRecommendedAction.StartEvent)
        {
            if (MessageBox.Show(
                    this,
                    "開始後才能進行正式題目。確定要開始活動嗎？",
                    "開始活動",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _ = await client.ChangeEventStateAsync(EventState.Active);
        }
        else if (recommendedAction == HostRecommendedAction.CompleteEvent)
        {
            if (MessageBox.Show(
                    this,
                    "完成後將關閉加入，並且無法再開始新題目。確定嗎？",
                    "完成活動",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            _ = await client.ChangeEventStateAsync(EventState.Completed);
        }

        await RecoverCurrentContextAsync();
        SetActionStatus("活動狀態已更新。");
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
            quizActivityView.ClearResult();
            return;
        }

        var statistics = await client.GetStatisticsAsync(quizState.SessionId.Value);
        var leaderboard = await client.GetLeaderboardAsync();
        quizActivityView.RenderResult(statistics, leaderboard, quizState.CorrectOptionId, quizState.Explanation);
        if (quizState.Mode == QuizQuestionMode.Practice &&
            !quizActivityView.HasNextInMode(QuizQuestionMode.Practice))
        {
            quizActivityView.ShowPracticeCompleted(statistics.ParticipantCount, statistics.AnsweredCount);
        }
    }

    private async void displayModeRequested(DisplayMode mode)
    {
        await RunViewOperationAsync(
            liveMonitorView.SetStatus,
            liveMonitorView.SetDisplayBusy,
            async () =>
            {
                await SetDisplayModeAsync(mode);
                liveMonitorView.SetStatus($"大螢幕已切換至 {displayMode}。");
            });
    }

    private async Task SetDisplayModeAsync(DisplayMode mode)
    {
        var state = await client.SetDisplayModeAsync(mode);
        displayMode = state.Mode;
        liveMonitorView.RenderDisplayMode(displayMode);
        RenderContext();
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
        quizActivityView.UpdateProgress(progress);
        RenderContext();
    }

    private void RenderContext()
    {
        eventManagementView.RenderConnectionState(connectionState);
        var selected = quizActivityView.SelectedQuestion;
        var position = GetQuestionPosition(selected);
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
        var hasQuiz = selected is not null || quizState.QuestionId.HasValue;
        currentActivityHeaderLabel.Text = hasQuiz ? "Activity: Quiz" : "Activity: —";
        activityStateHeaderLabel.Text = hasQuiz ? quizState.State.ToString().ToUpperInvariant() : "NO QUIZ";
        dashboardView.Render(
            currentEventName,
            currentQuizTitle,
            participants.Count,
            position,
            quizState.State,
            connectionState,
            displayMode,
            eventState,
            recommendedAction,
            quizState.AnsweredCount,
            quizState.OnlineCount);
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
        quizActivityView.Visible = view == HostView.Quiz;
        var selectedControl = view switch
        {
            HostView.Dashboard => (Control)dashboardView,
            HostView.Event => eventManagementView,
            HostView.QuestionBank => questionBankView,
            HostView.Quiz => quizActivityView,
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

    private string GetQuestionPosition(QuestionBankQuestionView? question)
    {
        if (question is null)
        {
            return "— / —";
        }

        var modeQuestions = questions.Where(item => item.Mode == question.Mode).ToArray();
        var index = Array.FindIndex(modeQuestions, item => item.Id == question.Id);
        var prefix = question.Mode == QuizQuestionMode.Practice ? "P" : "Q";
        return index < 0 ? "— / —" : $"{prefix}{index + 1} / {modeQuestions.Length}";
    }

    private void primaryActionButton_Click(object? sender, EventArgs e) =>
        primaryQuizActionRequested(sender, e);

    private void SetPrimaryActionBusy(bool isBusy)
    {
        primaryActionButton.Enabled = !isBusy && currentPrimaryAction != QuizPrimaryAction.None;
    }

    private void SetActionStatus(string message, bool isError = false)
    {
        actionStatusLabel.Text = message;
        actionStatusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
    }

    private void ResetCurrentEventPresentation()
    {
        participants.Clear();
        questionBanks = [];
        questions = [];
        quizState = CreateWaitingState();
        selectedQuestionId = null;
        currentEventName = "尚未選擇";
        currentQuizTitle = "尚未選擇";
        displayMode = DisplayMode.Waiting;
        eventState = EventState.Draft;
        isJoinOpen = false;
        recommendedAction = HostRecommendedAction.PrepareEvent;
        eventManagementView.ResetCurrentContext();
        questionBankView.RenderBanks([], null);
        quizActivityView.RenderQuestions([], null);
        quizActivityView.ClearResult();
        liveMonitorView.RenderDisplayMode(displayMode);
        RenderQuizState();
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
                SetActionStatus($"狀態同步失敗：{exception.Message}", true);
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
        new(QuizState.Waiting, null, null, null, null, [], null, null, 0, 0, 0, null, null);
}
