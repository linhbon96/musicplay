namespace MusicPlay.Api.DTOs;

public record TrackResponse(
    string Id,
    string Title,
    string ArtistName,
    string? Description,
    string? Genre,
    IReadOnlyCollection<string> Tags,
    double Duration,
    string StreamUrl,
    string? CoverUrl,
    bool Featured,
    long PlayCount,
    DateTime CreatedAt,
    bool IsApproved,
    DateTime? UpdatedAt,
    DateTime? ApprovedAt
);
