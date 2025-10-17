namespace MusicPlay.Api.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? DisplayName,
    string? Bio
);
