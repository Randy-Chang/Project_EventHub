(() => {
  "use strict";

  const form = document.getElementById("join-form");
  const status = document.getElementById("status");
  const joined = document.getElementById("joined");
  const welcome = document.getElementById("welcome");
  const eventIdInput = document.getElementById("event-id");
  let connection;

  const queryEventId = new URLSearchParams(window.location.search).get("eventId");
  if (queryEventId) {
    eventIdInput.value = queryEventId;
    restoreExistingSession(queryEventId);
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
      await connectSignalR(eventId, session);

      showJoined(result.participant);
      setStatus("已連線", false);
    } catch (error) {
      setStatus(error.message || "無法連線到活動 Server。", true);
      form.querySelector("button").disabled = false;
    }
  });

  async function connectSignalR(eventId, session) {
    if (connection) {
      await connection.stop();
    }

    const query = new URLSearchParams({
      role: "guest",
      eventId,
      participantId: session.participantId,
      token: session.token
    });

    connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/event?${query}`)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.onreconnecting(() => setStatus("連線中斷，正在重新連線…", true));
    connection.onreconnected(() => setStatus("已重新連線", false));
    connection.onclose(() => setStatus("已離線，請檢查 Wi-Fi 後重新整理。", true));
    await connection.start();
  }

  async function restoreExistingSession(eventId) {
    const storageKey = `eventhub.session.${eventId}`;
    const savedSession = readSession(storageKey);
    if (!savedSession?.participantId || !savedSession?.token) {
      return;
    }

    setStatus("正在恢復活動身份…", false);
    try {
      const response = await fetch(
        `/api/v1/events/${encodeURIComponent(eventId)}/participants/me?participantId=${encodeURIComponent(savedSession.participantId)}`,
        { headers: { "X-Participant-Token": savedSession.token } });
      if (!response.ok) {
        localStorage.removeItem(storageKey);
        setStatus("身份已失效，請重新加入活動。", true);
        return;
      }

      const participant = await response.json();
      await connectSignalR(eventId, savedSession);
      showJoined(participant);
      setStatus("已重新連線", false);
    } catch {
      setStatus("暫時無法恢復連線，請檢查 Wi-Fi 後重試。", true);
    }
  }

  function showJoined(participant) {
    welcome.textContent = `${participant.displayName}，歡迎加入！`;
    joined.hidden = false;
    form.hidden = true;
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
})();
