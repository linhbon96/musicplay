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

        var utcNow = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = CreateMd5Hash(request.Password),
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            Roles = isAdmin ? new List<string> { "Admin", "User" } : new List<string> { "User" },
            EmailConfirmed = isAdmin,
            EmailConfirmationToken = isAdmin ? null : Guid.NewGuid().ToString("N"),
            CreatedAt = utcNow,
            LastLoginAt = null
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
        if (!hash.Equals(user.PasswordHash, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await _context.Users.UpdateOneAsync(
            u => u.Id == user.Id,
            Builders<ApplicationUser>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow),
            cancellationToken: cancellationToken);

        return user;
    }

    public async Task<ApplicationUser?> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var filter = Builders<ApplicationUser>.Filter.And(
            Builders<ApplicationUser>.Filter.Eq(u => u.Email, normalizedEmail),
            Builders<ApplicationUser>.Filter.Eq(u => u.EmailConfirmationToken, token)
        );

        var update = Builders<ApplicationUser>.Update
            .Set(u => u.EmailConfirmed, true)
            .Set(u => u.EmailConfirmationToken, null)
            .Set(u => u.LastLoginAt, DateTime.UtcNow);

        return await _context.Users.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<ApplicationUser> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var count = await _context.Users.CountDocumentsAsync(x => x.Username == username, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var count = await _context.Users.CountDocumentsAsync(x => x.Email == normalizedEmail, cancellationToken: cancellationToken);
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
