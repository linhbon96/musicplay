using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicPlay.Api.Models;

public class ApplicationUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("displayName")]
    public string? DisplayName { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("links")]
    public Dictionary<string, string> Links { get; set; } = new();

    [BsonElement("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [BsonElement("emailConfirmationToken")]
    public string? EmailConfirmationToken { get; set; }

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}
