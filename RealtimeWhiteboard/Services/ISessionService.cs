using RealtimeWhiteboard.Models;

namespace RealtimeWhiteboard.Services;

public interface ISessionService
{
    Task<IReadOnlyList<WhiteboardSession>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WhiteboardSession?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WhiteboardSession> CreateAsync(WhiteboardSession session, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(WhiteboardSession session, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
