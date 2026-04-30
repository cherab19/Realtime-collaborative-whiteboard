namespace RealtimeWhiteboard.Hubs;

public record DrawingEventDto(
    string Tool,
    string ColorHex,
    int BrushSize,
    List<CoordinateDto> Coordinates,
    int? UserId,
    string UserDisplayName);

public record CoordinateDto(
    int X,
    int Y);
