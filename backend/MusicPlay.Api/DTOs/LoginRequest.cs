namespace MusicPlay.Api.DTOs;

public record LoginRequest(
    string Identifier,
    string Password
);
