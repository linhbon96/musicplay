namespace MusicPlay.Api.DTOs.Admin;

public record AdminOverviewResponse(
    AdminOverviewTotals Totals,
    IReadOnlyCollection<DailyUploadMetric> Uploads,
    IReadOnlyCollection<AdminTrackItem> PendingTracks
);

public record AdminOverviewTotals(long Tracks, long PendingTracks, long Users, long TotalPlays);

public record DailyUploadMetric(DateTime Date, long Count);

public record AdminTrackItem(
    string Id,
    string Title,
    string ArtistName,
    string? Genre,
    IReadOnlyCollection<string> Tags,
    double Duration,
    bool IsApproved,
    DateTime CreatedAt,
    string UploadedBy,
    long PlayCount
);
