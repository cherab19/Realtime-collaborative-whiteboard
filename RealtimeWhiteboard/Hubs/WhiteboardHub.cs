using Microsoft.AspNetCore.SignalR;
using RealtimeWhiteboard.Data;
using RealtimeWhiteboard.Models;

namespace RealtimeWhiteboard.Hubs;

public class WhiteboardHub(ApplicationDbContext dbContext) : Hub
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task JoinSession(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
        
        // Fetch history and send to the joining user
        var history = _dbContext.DrawingLogs
            .Where(log => log.WhiteboardSessionId == sessionId)
            .OrderBy(log => log.CreatedAtUtc)
            .ToList();

        await Clients.Caller.SendAsync("LoadHistory", history);
    }

    public async Task LeaveSession(int sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId.ToString());
    }

    public async Task Draw(int sessionId, string tool, string color, int size, string dataJson)
    {
        // Broadcast to others in the same session
        await Clients.OthersInGroup(sessionId.ToString()).SendAsync("ReceiveDraw", new
        {
            Tool = tool,
            ColorHex = color,
            BrushSize = size,
            DataJson = dataJson
        });

        // Save to database for persistence
        var log = new DrawingLog
        {
            WhiteboardSessionId = sessionId,
            Tool = tool,
            ColorHex = color,
            BrushSize = size,
            DataJson = dataJson,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.DrawingLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearCanvas(int sessionId)
    {
        await Clients.Group(sessionId.ToString()).SendAsync("ClearCanvas");

        // Remove history for this session
        var logs = _dbContext.DrawingLogs.Where(l => l.WhiteboardSessionId == sessionId);
        _dbContext.DrawingLogs.RemoveRange(logs);
        await _dbContext.SaveChangesAsync();
    }
}
