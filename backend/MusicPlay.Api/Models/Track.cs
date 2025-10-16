using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicPlay.Api.Models;

public class Track
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("artistName")]
    public string ArtistName { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("genre")]
    public string? Genre { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("duration")]
    public double Duration { get; set; }

    [BsonElement("filePath")]
    public string FilePath { get; set; } = null!;

    [BsonElement("coverUrl")]
    public string? CoverUrl { get; set; }

    [BsonElement("uploadedBy")]
    public string UploadedBy { get; set; } = null!;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("featured")]
    public bool Featured { get; set; }

    [BsonElement("playCount")]
    public long PlayCount { get; set; }

    [BsonElement("isApproved")]
    public bool IsApproved { get; set; }

    [BsonElement("approvedBy")]
    public string? ApprovedBy { get; set; }

    [BsonElement("approvedAt")]
    public DateTime? ApprovedAt { get; set; }
}
