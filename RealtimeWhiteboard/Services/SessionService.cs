using Microsoft.EntityFrameworkCore;
using RealtimeWhiteboard.Data;
using RealtimeWhiteboard.Models;

namespace RealtimeWhiteboard.Services;

public class SessionService(ApplicationDbContext dbContext) : ISessionService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<WhiteboardSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WhiteboardSessions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<WhiteboardSession?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WhiteboardSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<WhiteboardSession> CreateAsync(WhiteboardSession session, CancellationToken cancellationToken = default)
    {
        session.CreatedAtUtc = DateTime.UtcNow;

        _dbContext.WhiteboardSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task<bool> UpdateAsync(WhiteboardSession session, CancellationToken cancellationToken = default)
    {
        var existingSession = await _dbContext.WhiteboardSessions
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        if (existingSession is null)
        {
            return false;
        }

        existingSession.Name = session.Name;
        existingSession.Description = session.Description;
        existingSession.IsActive = session.IsActive;
        existingSession.ActiveUsers = session.ActiveUsers;
        existingSession.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingSession = await _dbContext.WhiteboardSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (existingSession is null)
        {
            return false;
        }

        _dbContext.WhiteboardSessions.Remove(existingSession);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
