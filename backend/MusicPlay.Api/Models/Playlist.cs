using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicPlay.Api.Models;

public class Playlist
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("trackIds")]
    public List<string> TrackIds { get; set; } = new();

    [BsonElement("coverUrl")]
    public string? CoverUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("ownerId")]
    public string OwnerId { get; set; } = null!;

    [BsonElement("isPublic")]
    public bool IsPublic { get; set; } = true;
}
