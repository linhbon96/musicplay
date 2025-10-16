using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicPlay.Api.Models;

public class ListeningHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("trackId")]
    public string TrackId { get; set; } = null!;

    [BsonElement("playedAt")]
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("genre")]
    public string? Genre { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
}
