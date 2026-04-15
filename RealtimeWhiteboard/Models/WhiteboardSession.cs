using System.ComponentModel.DataAnnotations;

namespace RealtimeWhiteboard.Models;

public class WhiteboardSession
{
    public int Id { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int ActiveUsers { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<DrawingLog> DrawingLogs { get; set; } = new List<DrawingLog>();
}
