using Microsoft.AspNetCore.StaticFiles;
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

    public async Task<Track> CreateAsync(TrackUploadRequest metadata, Stream fileStream, string fileName, string uploaderId, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{ObjectId.GenerateNewId()}{extension}";
        var path = Path.Combine(_storageOptions.UploadPath, safeFileName);

        await using (var file = File.Create(path))
        {
            await fileStream.CopyToAsync(file, cancellationToken);
        }

        var track = new Track
        {
            Title = metadata.Title,
            ArtistName = metadata.ArtistName,
            Description = metadata.Description,
            Genre = metadata.Genre,
            Tags = metadata.Tags,
            Duration = metadata.Duration,
            FilePath = path,
            UploadedBy = uploaderId,
            Featured = false,
            PlayCount = 0
        };

        await _context.Tracks.InsertOneAsync(track, cancellationToken: cancellationToken);
        return track;
    }

    public async Task<IReadOnlyCollection<Track>> GetFeaturedAsync(int limit, CancellationToken cancellationToken)
    {
        var tracks = await _context.Tracks
            .Find(t => t.Featured)
            .SortByDescending(t => t.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        return tracks;
    }

    public async Task<IReadOnlyCollection<Track>> GetLatestAsync(int limit, CancellationToken cancellationToken)
    {
        return await _context.Tracks
            .Find(FilterDefinition<Track>.Empty)
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
