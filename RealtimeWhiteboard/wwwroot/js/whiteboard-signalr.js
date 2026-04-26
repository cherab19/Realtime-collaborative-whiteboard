/**
 * SignalR Client Integration for Collaborative Whiteboard
 */

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/whiteboardHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

const statusEl = document.getElementById('connection-status');
const sessionSelect = document.getElementById('session-select');
const btnNewSession = document.getElementById('btn-new-session');

window.currentSessionId = null;

function updateStatus(isOnline, text) {
    statusEl.textContent = text;
    statusEl.className = isOnline ? 'status-online' : 'status-offline';
}

// ==========================================
// 1. SignalR Event Listeners
// ==========================================

function processIncomingData(data) {
    try {
        const payload = JSON.parse(data.dataJson);
        if (data.tool === 'action') {
            // New Object Model Event
            window.whiteboard.processRemoteEvent(payload.action, payload.element);
        } else {
            // Legacy Pixel Model Event (Backward Compatibility)
            const id = 'legacy-' + Math.random().toString(36).substring(7);
            const el = {
                id: id,
                type: data.tool,
                color: data.colorHex,
                size: data.brushSize,
                x1: payload.x1, y1: payload.y1,
                x2: payload.x2, y2: payload.y2,
                text: payload.text
            };
            if (data.tool === 'pen') {
                el.type = 'line'; // legacy pen was sent as a single line segment
            }
            window.whiteboard.processRemoteEvent('create', el);
        }
    } catch (e) {
        console.error("Failed to parse incoming data", e);
    }
}

connection.on("ReceiveDraw", (data) => {
    processIncomingData(data);
});

connection.on("LoadHistory", (historyLogs) => {
    window.whiteboard.clear();
    console.log(`Loading ${historyLogs.length} historical actions...`);
    historyLogs.forEach(log => processIncomingData(log));
});

connection.on("ClearCanvas", () => {
    window.whiteboard.clear();
});

connection.onreconnecting(error => updateStatus(false, "Reconnecting..."));
connection.onreconnected(connectionId => {
    updateStatus(true, "Connected");
    if (window.currentSessionId) joinSession(window.currentSessionId);
});
connection.onclose(error => updateStatus(false, "Disconnected"));

// ==========================================
// 2. Local Hooks (Sending Data)
// ==========================================

window.onLocalDraw = (tool, color, size, drawData) => {
    if (!window.currentSessionId || connection.state !== signalR.HubConnectionState.Connected) return;
    const dataJson = JSON.stringify(drawData);
    connection.invoke("Draw", window.currentSessionId, tool, color, size, dataJson)
        .catch(err => console.error("Error sending draw data:", err));
};

window.onLocalClear = () => {
    if (!window.currentSessionId || connection.state !== signalR.HubConnectionState.Connected) {
        window.whiteboard.clear(); 
        return;
    }
    connection.invoke("ClearCanvas", window.currentSessionId)
        .catch(err => console.error("Error sending clear canvas command:", err));
};

// ==========================================
// 3. Session Management
// ==========================================

async function loadSessions() {
    try {
        const response = await fetch('/api/sessions');
        if (response.ok) {
            const sessions = await response.json();
            sessionSelect.innerHTML = '<option value="">Select Session</option>';
            sessions.forEach(s => {
                const option = document.createElement('option');
                option.value = s.id;
                option.textContent = s.name;
                sessionSelect.appendChild(option);
            });
        }
    } catch (err) {
        console.error("Failed to load sessions", err);
    }
}

async function joinSession(sessionId) {
    if (!sessionId) return;
    try {
        if (window.currentSessionId && window.currentSessionId !== sessionId) {
            await connection.invoke("LeaveSession", window.currentSessionId);
            window.whiteboard.clear();
        }
        window.currentSessionId = parseInt(sessionId);
        const url = new URL(window.location);
        url.searchParams.set('sessionId', sessionId);
        window.history.pushState({}, '', url);
        
        if (connection.state !== signalR.HubConnectionState.Connected) {
            await connection.start();
        }
        await connection.invoke("JoinSession", window.currentSessionId);
    } catch (err) {
        console.error(`Error joining session ${sessionId}:`, err);
    }
}

sessionSelect.addEventListener('change', (e) => {
    const selectedId = e.target.value;
    if (selectedId) {
        joinSession(selectedId);
    } else {
        if (window.currentSessionId) {
            connection.invoke("LeaveSession", window.currentSessionId).catch(console.error);
            window.currentSessionId = null;
            window.whiteboard.clear();
            const url = new URL(window.location);
            url.searchParams.delete('sessionId');
            window.history.pushState({}, '', url);
        }
    }
});

btnNewSession.addEventListener('click', async () => {
    const sessionName = prompt("Enter a name for the new whiteboard session:");
    if (!sessionName || sessionName.trim() === '') return;
    try {
        const response = await fetch('/api/sessions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: sessionName })
        });
        if (response.ok) {
            const newSession = await response.json();
            await loadSessions(); 
            sessionSelect.value = newSession.id; 
            joinSession(newSession.id); 
        }
    } catch (err) {
        console.error("Error creating session", err);
    }
});

// ==========================================
// 4. Initialization
// ==========================================

async function init() {
    await loadSessions();
    try {
        await connection.start();
        updateStatus(true, "Connected");
        const params = new URLSearchParams(window.location.search);
        const persistedSessionId = params.get('sessionId');
        if (persistedSessionId) {
            sessionSelect.value = persistedSessionId;
            joinSession(persistedSessionId);
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        updateStatus(false, "Connection Failed");
    }
}

document.addEventListener('DOMContentLoaded', init);
