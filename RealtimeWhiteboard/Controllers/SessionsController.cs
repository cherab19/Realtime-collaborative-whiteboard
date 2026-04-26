using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RealtimeWhiteboard.Models;
using RealtimeWhiteboard.Services;

namespace RealtimeWhiteboard.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController(ISessionService sessionService, ILogger<SessionsController> logger) : ControllerBase
{
    private readonly ISessionService _sessionService = sessionService;
    private readonly ILogger<SessionsController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WhiteboardSessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WhiteboardSessionResponse>>> GetSessions(CancellationToken cancellationToken)
    {
        var sessions = await _sessionService.GetAllAsync(cancellationToken);

        return Ok(sessions.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WhiteboardSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WhiteboardSessionResponse>> GetSessionById(int id, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(id, cancellationToken);

        if (session is null)
        {
            return NotFound($"Session with id '{id}' was not found.");
        }

        return Ok(MapToResponse(session));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WhiteboardSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhiteboardSessionResponse>> CreateSession(
        [FromBody] CreateWhiteboardSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var session = new WhiteboardSession
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            ActiveUsers = 0
        };

        var createdSession = await _sessionService.CreateAsync(session, cancellationToken);
        _logger.LogInformation("Session {SessionId} created.", createdSession.Id);

        return CreatedAtAction(nameof(GetSessionById), new { id = createdSession.Id }, MapToResponse(createdSession));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSession(
        int id,
        [FromBody] UpdateWhiteboardSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sessionToUpdate = new WhiteboardSession
        {
            Id = id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            ActiveUsers = request.ActiveUsers
        };

        var isUpdated = await _sessionService.UpdateAsync(sessionToUpdate, cancellationToken);
        if (!isUpdated)
        {
            return NotFound($"Session with id '{id}' was not found.");
        }

        _logger.LogInformation("Session {SessionId} updated.", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(int id, CancellationToken cancellationToken)
    {
        var isDeleted = await _sessionService.DeleteAsync(id, cancellationToken);

        if (!isDeleted)
        {
            return NotFound($"Session with id '{id}' was not found.");
        }

        _logger.LogInformation("Session {SessionId} deleted.", id);
        return NoContent();
    }

    private static WhiteboardSessionResponse MapToResponse(WhiteboardSession session)
    {
        return new WhiteboardSessionResponse(
            session.Id,
            session.Name,
            session.Description,
            session.ActiveUsers,
            session.IsActive,
            session.CreatedAtUtc,
            session.UpdatedAtUtc);
    }
}

public record CreateWhiteboardSessionRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Name,
    [StringLength(300)] string? Description);

public record UpdateWhiteboardSessionRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Name,
    [StringLength(300)] string? Description,
    bool IsActive,
    [Range(0, int.MaxValue)] int ActiveUsers);

public record WhiteboardSessionResponse(
    int Id,
    string Name,
    string? Description,
    int ActiveUsers,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
