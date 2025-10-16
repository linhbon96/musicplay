namespace MusicPlay.Api.DTOs;

public record PlaylistResponse(
    string Id,
    string Title,
    string? Description,
    IReadOnlyCollection<TrackResponse> Tracks,
    string? CoverUrl,
    bool IsPublic
);
