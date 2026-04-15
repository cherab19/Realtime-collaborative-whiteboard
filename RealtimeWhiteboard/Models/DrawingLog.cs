using System.ComponentModel.DataAnnotations;

namespace RealtimeWhiteboard.Models;

public class DrawingLog
{
    public int Id { get; set; }

    [Required]
    public int WhiteboardSessionId { get; set; }

    public WhiteboardSession? WhiteboardSession { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [StringLength(40)]
    public string Tool { get; set; } = "freehand";

    [Required]
    [StringLength(20)]
    public string ColorHex { get; set; } = "#000000";

    [Range(1, 100)]
    public int BrushSize { get; set; } = 2;

    [Required]
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
