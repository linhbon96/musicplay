using Microsoft.AspNetCore.StaticFiles;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MusicPlay.Api.Configuration;
using MusicPlay.Api.DTOs;
using MusicPlay.Api.Infrastructure;
using MusicPlay.Api.Models;

namespace MusicPlay.Api.Services;

public class TrackService
{
    private readonly MongoContext _context;
    private readonly StorageOptions _storageOptions;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public TrackService(MongoContext context, StorageOptions storageOptions)
    {
        _context = context;
        _storageOptions = storageOptions;
        Directory.CreateDirectory(_storageOptions.UploadPath);
    }

    public async Task<Track> CreateAsync(TrackUploadRequest metadata, Stream fileStream, string fileName, string uploaderId, bool autoApprove, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{ObjectId.GenerateNewId()}{extension}";
        var path = Path.Combine(_storageOptions.UploadPath, safeFileName);

        await using (var file = File.Create(path))
        {
            await fileStream.CopyToAsync(file, cancellationToken);
        }

        var utcNow = DateTime.UtcNow;
        var track = new Track
        {
            Title = metadata.Title,
            ArtistName = metadata.ArtistName,
            Description = metadata.Description,
            Genre = metadata.Genre,
            Tags = metadata.Tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).ToList() ?? new List<string>(),
            Duration = metadata.Duration,
            FilePath = path,
            UploadedBy = uploaderId,
            Featured = false,
            PlayCount = 0,
            IsApproved = autoApprove,
            ApprovedBy = autoApprove ? uploaderId : null,
            ApprovedAt = autoApprove ? utcNow : null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _context.Tracks.InsertOneAsync(track, cancellationToken: cancellationToken);
        return track;
    }

    public async Task<IReadOnlyCollection<Track>> GetFeaturedAsync(int limit, CancellationToken cancellationToken)
    {
        var filter = Builders<Track>.Filter.And(
            Builders<Track>.Filter.Eq(t => t.Featured, true),
            Builders<Track>.Filter.Eq(t => t.IsApproved, true));

        var tracks = await _context.Tracks
            .Find(filter)
            .SortByDescending(t => t.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        return tracks;
    }

    public async Task<IReadOnlyCollection<Track>> GetLatestAsync(int limit, CancellationToken cancellationToken)
    {
        var filter = Builders<Track>.Filter.Eq(t => t.IsApproved, true);
        return await _context.Tracks
            .Find(filter)
            .SortByDescending(t => t.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Track?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _context.Tracks
            .Find(t => t.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ApproveTrackAsync(string trackId, string approverId, CancellationToken cancellationToken)
    {
        var update = Builders<Track>.Update
            .Set(t => t.IsApproved, true)
            .Set(t => t.ApprovedBy, approverId)
            .Set(t => t.ApprovedAt, DateTime.UtcNow)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Tracks.UpdateOneAsync(t => t.Id == trackId, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task RecordPlayAsync(Track track, string? userId, CancellationToken cancellationToken)
    {
        var update = Builders<Track>.Update
            .Inc(t => t.PlayCount, 1)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        await _context.Tracks.UpdateOneAsync(t => t.Id == track.Id, update, cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(userId) || userId == "guest")
        {
            return;
        }

        var history = new ListeningHistory
        {
            UserId = userId,
            TrackId = track.Id,
            Genre = track.Genre,
            Tags = track.Tags,
            PlayedAt = DateTime.UtcNow
        };

        await _context.ListeningHistories.InsertOneAsync(history, cancellationToken: cancellationToken);
    }

    public bool TryGetContentType(string fileName, out string contentType)
    {
        if (!_contentTypeProvider.TryGetContentType(fileName, out contentType!))
        {
            contentType = "audio/mpeg";
            return false;
        }

        return true;
    }

    public FileStream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public string BuildStreamUrl(Track track) => $"/api/tracks/{track.Id}/stream";
}
