using Microsoft.AspNetCore.SignalR;
using RealtimeWhiteboard.Data;
using RealtimeWhiteboard.Models;

namespace RealtimeWhiteboard.Hubs;

/// <summary>
/// SignalR hub for real-time whiteboard collaboration.
/// Handles drawing events, session management, and persistence.
/// </summary>
public class WhiteboardHub(ApplicationDbContext dbContext, ILogger<WhiteboardHub> logger) : Hub
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<WhiteboardHub> _logger = logger;

    /// <summary>
    /// Called when a user joins a whiteboard session.
    /// Adds the connection to a session group and broadcasts user-joined event.
    /// </summary>
    public async Task JoinSession(int sessionId, string userDisplayName)
    {
        var groupName = sessionId.ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {UserDisplayName} (connection {ConnectionId}) joined session {SessionId}.",
            userDisplayName, Context.ConnectionId, sessionId);

        // Fetch and send drawing history to the joining user
        try
        {
            var history = _dbContext.DrawingLogs
                .Where(log => log.WhiteboardSessionId == sessionId)
                .OrderBy(log => log.CreatedAtUtc)
                .Select(log => new
                {
                    log.Id,
                    log.Tool,
                    log.ColorHex,
                    log.BrushSize,
                    log.DataJson,
                    log.CreatedAtUtc
                })
                .ToList();

            await Clients.Caller.SendAsync("LoadHistory", history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading history for session {SessionId}.", sessionId);
        }

        // Notify all clients in the session about the new user
        await Clients.Group(groupName).SendAsync("UserJoined", new
        {
            userDisplayName,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Called when a user leaves a whiteboard session.
    /// Removes the connection from the session group and broadcasts user-left event.
    /// </summary>
    public async Task LeaveSession(int sessionId, string userDisplayName)
    {
        var groupName = sessionId.ToString();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {UserDisplayName} (connection {ConnectionId}) left session {SessionId}.",
            userDisplayName, Context.ConnectionId, sessionId);

        // Notify remaining clients about the user leaving
        await Clients.Group(groupName).SendAsync("UserLeft", new
        {
            userDisplayName,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Called when a user draws on the canvas.
    /// Broadcasts the drawing to all other users and persists to database.
    /// </summary>
    public async Task SendDrawing(int sessionId, DrawingEventDto drawingEvent)
    {
        if (drawingEvent == null || string.IsNullOrWhiteSpace(drawingEvent.Tool))
        {
            _logger.LogWarning(
                "Received invalid drawing event from connection {ConnectionId}.",
                Context.ConnectionId);
            return;
        }

        try
        {
            var groupName = sessionId.ToString();

            _logger.LogDebug(
                "Broadcasting drawing ({Tool}, {BrushSize}px) in session {SessionId} from user {UserDisplayName}.",
                drawingEvent.Tool, drawingEvent.BrushSize, sessionId, drawingEvent.UserDisplayName);

            // Broadcast to all clients in the session
            await Clients.Group(groupName).SendAsync("ReceiveDrawing", new
            {
                drawingEvent,
                connectionId = Context.ConnectionId,
                timestamp = DateTime.UtcNow
            });

            // Persist to database
            var dataJson = System.Text.Json.JsonSerializer.Serialize(drawingEvent.Coordinates);
            var log = new DrawingLog
            {
                WhiteboardSessionId = sessionId,
                UserId = drawingEvent.UserId,
                Tool = drawingEvent.Tool,
                ColorHex = drawingEvent.ColorHex,
                BrushSize = drawingEvent.BrushSize,
                DataJson = dataJson,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.DrawingLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing drawing in session {SessionId}.", sessionId);
        }
    }

    /// <summary>
    /// Called when a user clears the canvas.
    /// Broadcasts the clear event and removes drawing history from the database.
    /// </summary>
    public async Task ClearCanvas(int sessionId)
    {
        try
        {
            var groupName = sessionId.ToString();

            _logger.LogInformation("Canvas cleared in session {SessionId}.", sessionId);

            // Broadcast clear event to all clients
            await Clients.Group(groupName).SendAsync("CanvasCleared", new
            {
                timestamp = DateTime.UtcNow
            });

            // Remove drawing history from database
            var logs = _dbContext.DrawingLogs.Where(l => l.WhiteboardSessionId == sessionId);
            _dbContext.DrawingLogs.RemoveRange(logs);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing canvas in session {SessionId}.", sessionId);
        }
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Logs the connection event.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Client {ConnectionId} connected to WhiteboardHub.",
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Logs the disconnection event and removes the connection from all groups.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client {ConnectionId} disconnected from WhiteboardHub. Exception: {ExceptionMessage}",
            Context.ConnectionId,
            exception?.Message ?? "None");

        await base.OnDisconnectedAsync(exception);
    }
}
