(() => {
  "use strict";

  const form = document.getElementById("join-form");
  const status = document.getElementById("status");
  const joined = document.getElementById("joined");
  const welcome = document.getElementById("welcome");
  const eventIdInput = document.getElementById("event-id");
  const eventIdField = document.getElementById("event-id-field");
  const eventName = document.getElementById("event-name");
  const quizQuestion = document.getElementById("quiz-question");
  const quizCountdown = document.getElementById("quiz-countdown");
  const quizOptions = document.getElementById("quiz-options");
  const quizMessage = document.getElementById("quiz-message");
  const quizScore = document.getElementById("quiz-score");
  const leaderboard = document.getElementById("leaderboard");
  const leaderboardList = document.getElementById("leaderboard-list");
  let connection;
  let activeEventId;
  let activeSession;
  let countdownTimer;
  let recoveryPollingTimer;
  let reconnectLoopPromise;
  let quizStateRequestVersion = 0;
  let serverClockOffsetMilliseconds = 0;

  const quizState = Object.freeze({ waiting: 0, open: 1, closed: 2, revealed: 3 });
  const reconnectDelays = [0, 2000, 5000, 10000, 30000];
  const reconnectPolicy = {
    nextRetryDelayInMilliseconds: (context) =>
      reconnectDelays[Math.min(context.previousRetryCount, reconnectDelays.length - 1)]
  };

  const queryEventId = new URLSearchParams(window.location.search).get("eventId");
  const queryJoinCode = new URLSearchParams(window.location.search).get("joinCode");
  if (queryJoinCode) {
    form.hidden = true;
    resolveJoinCode(queryJoinCode);
  } else if (queryEventId) {
    eventIdInput.value = queryEventId;
    restoreExistingSession(queryEventId);
  }

  async function resolveJoinCode(joinCode) {
    setStatus("正在取得活動資訊…", false);
    try {
      const response = await fetch(`/api/v1/events/join/${encodeURIComponent(joinCode)}`);
      if (!response.ok) {
        const problem = await response.json();
        throw new Error(problem.detail || "找不到此活動，請確認 QR Code 或加入網址是否正確。");
      }

      const joinInfo = await response.json();
      eventName.textContent = joinInfo.eventName;
      eventName.hidden = false;
      eventIdInput.value = joinInfo.eventId;
      eventIdField.hidden = true;
      const restored = await restoreExistingSession(joinInfo.eventId);
      if (restored) {
        return;
      }

      if (!joinInfo.isJoinOpen) {
        setStatus("此活動目前已停止加入。", true);
        return;
      }

      form.hidden = false;
      setStatus("請填寫資料加入活動", false);
    } catch (error) {
      form.hidden = true;
      setStatus(error.message || "找不到此活動，請確認 QR Code 或加入網址是否正確。", true);
    }
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const eventId = eventIdInput.value.trim();
    const storageKey = `eventhub.session.${eventId}`;
    let savedSession = readSession(storageKey);
    if (!savedSession?.token) {
      savedSession = { token: createSessionToken() };
      localStorage.setItem(storageKey, JSON.stringify(savedSession));
    }

    setStatus("正在加入活動…", false);
    form.querySelector("button").disabled = true;

    try {
      const response = await fetch(`/api/v1/events/${encodeURIComponent(eventId)}/participants/join`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: document.getElementById("name").value,
          nickname: document.getElementById("nickname").value || null,
          employeeNumber: document.getElementById("employee-number").value || null,
          department: document.getElementById("department").value || null,
          tableNumber: document.getElementById("table-number").value || null,
          sessionToken: savedSession.token
        })
      });

      if (!response.ok) {
        const problem = await response.json();
        throw new Error(problem.detail || "加入活動失敗。");
      }

      const result = await response.json();
      const session = {
        participantId: result.participant.id,
        token: result.sessionToken
      };
      localStorage.setItem(storageKey, JSON.stringify(session));
      const isRealtimeConnected = await connectSignalR(eventId, session);

      showJoined(result.participant);
      await loadCurrentQuizState(eventId, session);
      setStatus(
        isRealtimeConnected ? "已連線" : "已加入；即時連線恢復中，題目狀態會自動同步。",
        !isRealtimeConnected);
    } catch (error) {
      setStatus(error.message || "無法連線到活動 Server。", true);
      form.querySelector("button").disabled = false;
    }
  });

  async function connectSignalR(eventId, session) {
    const previousConnection = connection;
    connection = null;
    if (previousConnection) {
      await previousConnection.stop();
    }

    const query = new URLSearchParams({
      role: "guest",
      eventId,
      participantId: session.participantId,
      token: session.token
    });

    const nextConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/event?${query}`)
      .withAutomaticReconnect(reconnectPolicy)
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection = nextConnection;
    activeEventId = eventId;
    activeSession = session;
    nextConnection.onreconnecting(() => {
      if (connection !== nextConnection) {
        return;
      }

      setStatus("即時連線中斷，正在重新連線；題目狀態會持續同步。", true);
      startRecoveryPolling(eventId, session);
    });
    nextConnection.onreconnected(async () => {
      if (connection !== nextConnection) {
        return;
      }

      stopRecoveryPolling();
      setStatus("已重新連線", false);
      await loadCurrentQuizState(eventId, session);
    });
    nextConnection.onclose(() => {
      if (connection !== nextConnection) {
        return;
      }

      setStatus("即時連線已中斷，正在恢復；不需要重新整理頁面。", true);
      startRecoveryPolling(eventId, session);
      startDisconnectedConnectionLoop(nextConnection, eventId, session);
    });
    nextConnection.on("QuestionStarted", () => loadCurrentQuizState(eventId, session));
    nextConnection.on("QuestionClosed", () => loadCurrentQuizState(eventId, session));
    nextConnection.on("AnswerRevealed", () => loadCurrentQuizState(eventId, session));
    nextConnection.on("LeaderboardUpdated", () => loadLeaderboard(eventId, session));

    try {
      await nextConnection.start();
      stopRecoveryPolling();
      return true;
    } catch {
      startRecoveryPolling(eventId, session);
      startDisconnectedConnectionLoop(nextConnection, eventId, session);
      return false;
    }
  }

  function startRecoveryPolling(eventId, session) {
    if (recoveryPollingTimer) {
      return;
    }

    loadCurrentQuizState(eventId, session);
    recoveryPollingTimer = window.setInterval(() => {
      if (!document.hidden && activeEventId === eventId && activeSession === session) {
        loadCurrentQuizState(eventId, session);
      }
    }, 5000);
  }

  function stopRecoveryPolling() {
    window.clearInterval(recoveryPollingTimer);
    recoveryPollingTimer = undefined;
  }

  function startDisconnectedConnectionLoop(expectedConnection, eventId, session) {
    if (reconnectLoopPromise?.connection === expectedConnection) {
      return;
    }

    const loop = (async () => {
      let attempt = 0;
      while (connection === expectedConnection &&
             expectedConnection.state === signalR.HubConnectionState.Disconnected) {
        const delay = reconnectDelays[Math.min(attempt, reconnectDelays.length - 1)];
        if (delay > 0) {
          await wait(delay);
        }

        try {
          await expectedConnection.start();
          if (connection !== expectedConnection) {
            return;
          }

          stopRecoveryPolling();
          setStatus("已重新連線", false);
          await loadCurrentQuizState(eventId, session);
          return;
        } catch {
          attempt++;
        }
      }
    });
    const trackedLoop = { connection: expectedConnection, promise: loop() };
    reconnectLoopPromise = trackedLoop;
    trackedLoop.promise.finally(() => {
      if (reconnectLoopPromise === trackedLoop) {
        reconnectLoopPromise = undefined;
      }
    });
  }

  function wait(milliseconds) {
    return new Promise((resolve) => window.setTimeout(resolve, milliseconds));
  }

  async function restoreExistingSession(eventId) {
    const storageKey = `eventhub.session.${eventId}`;
    const savedSession = readSession(storageKey);
    if (!savedSession?.participantId || !savedSession?.token) {
      return false;
    }

    setStatus("正在恢復活動身份…", false);
    try {
      const response = await fetch(
        `/api/v1/events/${encodeURIComponent(eventId)}/participants/me?participantId=${encodeURIComponent(savedSession.participantId)}`,
        { headers: { "X-Participant-Token": savedSession.token } });
      if (!response.ok) {
        localStorage.removeItem(storageKey);
        setStatus("身份已失效，請重新加入活動。", true);
        return false;
      }

      const participant = await response.json();
      const isRealtimeConnected = await connectSignalR(eventId, savedSession);
      showJoined(participant);
      await loadCurrentQuizState(eventId, savedSession);
      setStatus(
        isRealtimeConnected ? "已重新連線" : "身份已恢復；即時連線恢復中。",
        !isRealtimeConnected);
      return true;
    } catch {
      setStatus("暫時無法恢復連線，請檢查 Wi-Fi 後重試。", true);
      return false;
    }
  }

  function showJoined(participant) {
    welcome.textContent = `${participant.displayName}，歡迎加入！`;
    joined.hidden = false;
    form.hidden = true;
  }

  async function loadCurrentQuizState(eventId, session) {
    const requestVersion = ++quizStateRequestVersion;
    try {
      const response = await fetch(
        `/api/v1/events/${encodeURIComponent(eventId)}/quiz/current?participantId=${encodeURIComponent(session.participantId)}`,
        { headers: { "X-Participant-Token": session.token } });
      if (!response.ok) {
        const problem = await response.json();
        throw new Error(problem.detail || "無法取得目前題目。");
      }

      const state = await response.json();
      if (requestVersion === quizStateRequestVersion) {
        renderQuizState(state);
      }
    } catch (error) {
      if (requestVersion === quizStateRequestVersion) {
        quizMessage.textContent = error.message || "題目狀態同步失敗。";
        quizMessage.classList.add("error");
      }
    }
  }

  function renderQuizState(state) {
    clearInterval(countdownTimer);
    console.log("Countdown stopped: Quiz state render");
    console.log("QuizState =", state);
    console.log("QuestionState =", state.state);
    console.log("DeadlineUtc =", state.answerDeadlineUtc);
    console.log("BrowserNowUtc =", new Date().toISOString());
    updateServerClockOffset(state.serverTimeUtc);
    const deadline = parseDeadline(state.answerDeadlineUtc);
    const remainingMilliseconds = deadline
      ? deadline.getTime() - getServerAdjustedNowMilliseconds()
      : Number.NaN;
    console.log("RemainingMs =", remainingMilliseconds);
    quizOptions.replaceChildren();
    quizMessage.classList.remove("error", "correct", "incorrect");
    quizScore.hidden = true;
    leaderboard.hidden = true;
    quizCountdown.hidden = true;

    if (state.state === quizState.waiting || !state.sessionId) {
      quizQuestion.textContent = "等待下一題…";
      quizMessage.textContent = "主持人開始題目後會自動顯示。";
      return;
    }

    const isPractice = state.mode === "Practice";
    quizQuestion.textContent = isPractice ? `操作練習｜${state.questionText}` : state.questionText;
    const optionById = new Map(state.options.map((option) => [option.id, option]));
    state.options.forEach((option, index) => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "quiz-option";
      button.textContent = `${String.fromCharCode(65 + index)}. ${option.text}`;
      const disableReason = getAnswerDisableReason(state, deadline, remainingMilliseconds);
      button.disabled = disableReason !== null;
      console.log(button.disabled ? `Answer disabled: ${disableReason}` : "Answer enabled");
      if (option.id === state.selectedOptionId) {
        button.classList.add("selected");
      }
      if (state.state === quizState.revealed && option.id === state.correctOptionId) {
        button.classList.add("answer-correct");
      }
      button.addEventListener("click", () => submitAnswer(state.sessionId, option.id));
      quizOptions.appendChild(button);
    });

    if (state.state === quizState.open) {
      if (state.hasAnswered) {
        quizMessage.textContent = "答案已送出，等待其他人作答。";
      } else {
        quizMessage.textContent = "請選擇一個答案。";
      }
      startCountdown(deadline);
      return;
    }

    if (state.state === quizState.closed) {
      quizMessage.textContent = "本題作答結束，等待主持人公布答案。";
      return;
    }

    const correct = optionById.get(state.correctOptionId);
    const selected = optionById.get(state.selectedOptionId);
    const correctLabel = correct ? `${String.fromCharCode(65 + correct.order)}. ${correct.text}` : "未提供";
    const selectedLabel = selected ? `${String.fromCharCode(65 + selected.order)}. ${selected.text}` : "未作答";
    const resultLabel = !selected ? "未作答" : (state.isCorrect === true ? "答對！" : "答錯");
    quizMessage.textContent = `正確答案：${correctLabel}　你的答案：${selectedLabel}　${resultLabel}`;
    quizMessage.classList.add(state.isCorrect === true ? "correct" : "incorrect");
    const questionScore = state.questionScore ?? 0;
    const baseScore = state.baseScore ?? 0;
    const speedBonus = state.speedBonus ?? 0;
    const explanation = state.explanation ? `　說明：${state.explanation}` : "";
    quizScore.textContent = isPractice
      ? `本題不計分　正式累積 ${state.totalScore ?? 0} 分${explanation}`
      : `本題 ${questionScore} 分（答對 ${baseScore} + 速度 ${speedBonus}）　累積 ${state.totalScore ?? 0} 分　第 ${state.rank ?? "-"} 名${explanation}`;
    quizScore.hidden = false;
    if (activeEventId && activeSession) {
      loadLeaderboard(activeEventId, activeSession);
    }
  }

  async function loadLeaderboard(eventId, session) {
    try {
      const response = await fetch(
        `/api/v1/events/${encodeURIComponent(eventId)}/quiz/leaderboard?top=5&participantId=${encodeURIComponent(session.participantId)}`,
        { headers: { "X-Participant-Token": session.token } });
      if (!response.ok) {
        return;
      }

      const result = await response.json();
      leaderboardList.replaceChildren();
      result.entries.forEach((entry) => {
        const item = document.createElement("li");
        item.textContent = `${entry.displayName}　${entry.totalScore} 分`;
        if (entry.participantId === session.participantId) {
          item.classList.add("is-me");
        }
        leaderboardList.appendChild(item);
      });
      leaderboard.hidden = result.entries.length === 0;
    } catch {
      // Current score remains available even if this optional list cannot be refreshed.
    }
  }

  function updateServerClockOffset(serverTimeUtc) {
    const serverTime = new Date(serverTimeUtc);
    serverClockOffsetMilliseconds = Number.isNaN(serverTime.getTime())
      ? 0
      : serverTime.getTime() - Date.now();
  }

  function getServerAdjustedNowMilliseconds() {
    return Date.now() + serverClockOffsetMilliseconds;
  }

  function parseDeadline(deadlineUtc) {
    if (!deadlineUtc) {
      return null;
    }

    const deadline = new Date(deadlineUtc);
    return Number.isNaN(deadline.getTime()) ? null : deadline;
  }

  function getAnswerDisableReason(state, deadline, remainingMilliseconds) {
    if (state.state !== quizState.open) {
      return "Question not open";
    }
    if (!deadline) {
      return "Missing or invalid deadline";
    }
    if (remainingMilliseconds <= 0) {
      return "Deadline expired";
    }
    if (state.hasAnswered) {
      return "Already answered";
    }
    return null;
  }

  function startCountdown(deadline) {
    if (!deadline) {
      console.log("Countdown stopped: Missing or invalid deadline");
      return;
    }

    console.log("Countdown started");
    quizCountdown.hidden = false;
    const update = () => {
      const remainingMilliseconds = deadline.getTime() - getServerAdjustedNowMilliseconds();
      const seconds = Math.max(0, Math.ceil(remainingMilliseconds / 1000));
      quizCountdown.textContent = `剩餘 ${String(seconds).padStart(2, "0")} 秒`;
      if (remainingMilliseconds <= 0) {
        clearInterval(countdownTimer);
        console.log("Countdown stopped: Deadline expired");
        if (activeEventId && activeSession) {
          loadCurrentQuizState(activeEventId, activeSession);
        }
      }
    };
    countdownTimer = setInterval(update, 250);
    update();
  }

  async function submitAnswer(sessionId, selectedOptionId) {
    if (!activeEventId || !activeSession) {
      return;
    }

    quizOptions.querySelectorAll("button").forEach((button) => { button.disabled = true; });
    quizMessage.textContent = "正在送出答案…";
    try {
      const response = await fetch(
        `/api/v1/events/${encodeURIComponent(activeEventId)}/quiz/sessions/${encodeURIComponent(sessionId)}/answers`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            "X-Participant-Token": activeSession.token
          },
          body: JSON.stringify({
            participantId: activeSession.participantId,
            selectedOptionId
          })
        });
      if (!response.ok) {
        const problem = await response.json();
        throw new Error(problem.detail || "答案送出失敗。 ");
      }

      await loadCurrentQuizState(activeEventId, activeSession);
    } catch (error) {
      quizMessage.textContent = error.message || "答案送出失敗。";
      quizMessage.classList.add("error");
      await loadCurrentQuizState(activeEventId, activeSession);
    }
  }

  function readSession(key) {
    try {
      return JSON.parse(localStorage.getItem(key));
    } catch {
      localStorage.removeItem(key);
      return null;
    }
  }

  function createSessionToken() {
    const bytes = crypto.getRandomValues(new Uint8Array(32));
    let binary = "";
    bytes.forEach((value) => { binary += String.fromCharCode(value); });
    return btoa(binary).replaceAll("+", "-").replaceAll("/", "_").replace(/=+$/, "");
  }

  function setStatus(message, isError) {
    status.textContent = message;
    status.classList.toggle("error", isError);
  }

  document.addEventListener("visibilitychange", () => {
    if (!document.hidden && activeEventId && activeSession) {
      loadCurrentQuizState(activeEventId, activeSession);
      if (connection?.state === signalR.HubConnectionState.Disconnected) {
        startRecoveryPolling(activeEventId, activeSession);
        startDisconnectedConnectionLoop(connection, activeEventId, activeSession);
      }
    }
  });

  window.addEventListener("online", () => {
    if (activeEventId && activeSession) {
      loadCurrentQuizState(activeEventId, activeSession);
      if (connection?.state === signalR.HubConnectionState.Disconnected) {
        startRecoveryPolling(activeEventId, activeSession);
        startDisconnectedConnectionLoop(connection, activeEventId, activeSession);
      }
    }
  });
})();
