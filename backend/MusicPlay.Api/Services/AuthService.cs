using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using MusicPlay.Api.DTOs;
using MusicPlay.Api.Infrastructure;
using MusicPlay.Api.Models;

namespace MusicPlay.Api.Services;

public class AuthService
{
    private readonly MongoContext _context;

    public AuthService(MongoContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<ApplicationUser>.Filter.Or(
            Builders<ApplicationUser>.Filter.Eq(x => x.Username, identifier),
            Builders<ApplicationUser>.Filter.Eq(x => x.Email, identifier)
        );

        return await _context.Users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApplicationUser> RegisterAsync(RegisterRequest request, bool isAdmin, CancellationToken cancellationToken)
    {
        if (await UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        if (await EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var user = new ApplicationUser
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = CreateMd5Hash(request.Password),
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            Roles = isAdmin ? new List<string> { "Admin", "User" } : new List<string> { "User" }
        };

        await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<ApplicationUser?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var hash = CreateMd5Hash(request.Password);
        return hash.Equals(user.PasswordHash, StringComparison.OrdinalIgnoreCase) ? user : null;
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var count = await _context.Users.CountDocumentsAsync(x => x.Username == username, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var count = await _context.Users.CountDocumentsAsync(x => x.Email == email, cancellationToken: cancellationToken);
        return count > 0;
    }

    public static string CreateMd5Hash(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
