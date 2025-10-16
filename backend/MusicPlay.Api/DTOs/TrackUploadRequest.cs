namespace MusicPlay.Api.DTOs;

public class TrackUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public List<string> Tags { get; set; } = new();
    public double Duration { get; set; }
}
