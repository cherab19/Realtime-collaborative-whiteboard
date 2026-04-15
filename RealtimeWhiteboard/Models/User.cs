using System.ComponentModel.DataAnnotations;

namespace RealtimeWhiteboard.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(128)]
    public string? ConnectionId { get; set; }

    public bool IsOnline { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
