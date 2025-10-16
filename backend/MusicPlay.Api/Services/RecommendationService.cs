using Microsoft.ML;
using MongoDB.Driver;
using MusicPlay.Api.Infrastructure;
using MusicPlay.Api.Models;
using System.Linq;

namespace MusicPlay.Api.Services;

public class RecommendationService
{
    private readonly MongoContext _context;
    private readonly MLContext _mlContext = new();

    public RecommendationService(MongoContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Track>> GetPersonalizedRecommendationsAsync(string userId, int limit, CancellationToken cancellationToken)
    {
        var history = await _context.ListeningHistories
            .Find(h => h.UserId == userId)
            .SortByDescending(h => h.PlayedAt)
            .Limit(200)
            .ToListAsync(cancellationToken);

        var approvedFilter = Builders<Track>.Filter.Eq(t => t.IsApproved, true);
        var allTracks = await _context.Tracks
            .Find(approvedFilter)
            .ToListAsync(cancellationToken);

        if (!allTracks.Any())
        {
            return Array.Empty<Track>();
        }

        if (!history.Any())
        {
            return allTracks
                .OrderByDescending(t => t.PlayCount)
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToList();
        }

        var trackContent = allTracks.Select(track => new TrackVectorInput
        {
            TrackId = track.Id,
            Content = BuildContent(track)
        }).ToList();

        var dataView = _mlContext.Data.LoadFromEnumerable(trackContent);
        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(TrackVectorInput.Content));
        var model = pipeline.Fit(dataView);
        var transformed = model.Transform(dataView);
        var trackVectors = _mlContext.Data.CreateEnumerable<TrackVector>(transformed, reuseRowObject: false).ToList();

        var preferredTags = history.SelectMany(h => h.Tags ?? new List<string>()).ToList();
        var preferredGenres = history
            .Select(h => h.Genre)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        var userProfileContent = string.Join(" ", preferredTags.Concat(preferredGenres));
        if (string.IsNullOrWhiteSpace(userProfileContent))
        {
            userProfileContent = string.Join(" ", history.Select(h => h.TrackId));
        }

        var userDataView = _mlContext.Data.LoadFromEnumerable(new[]
        {
            new TrackVectorInput { TrackId = "user-profile", Content = userProfileContent }
        });

        var userTransformed = model.Transform(userDataView);
        var userVector = _mlContext.Data.CreateEnumerable<TrackVector>(userTransformed, reuseRowObject: false).FirstOrDefault();
        if (userVector is null || userVector.Features is null)
        {
            return allTracks
                .OrderByDescending(t => t.PlayCount)
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToList();
        }

        var listened = history.Select(h => h.TrackId).ToHashSet();

        var scored = new List<(Track Track, double Score)>();
        foreach (var trackVector in trackVectors)
        {
            if (trackVector.TrackId == "user-profile" || !listened.Add(trackVector.TrackId))
            {
                continue;
            }

            var track = allTracks.FirstOrDefault(t => t.Id == trackVector.TrackId);
            if (track is null || trackVector.Features is null)
            {
                continue;
            }

            var score = CosineSimilarity(userVector.Features, trackVector.Features);
            scored.Add((track, score));
        }

        return scored
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Track.PlayCount)
            .ThenByDescending(x => x.Track.CreatedAt)
            .Take(limit)
            .Select(x => x.Track)
            .ToList();
    }

    private static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            return 0;
        }

        double dot = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (var i = 0; i < vectorA.Length; i++)
        {
            dot += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    private static string BuildContent(Track track)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(track.Genre))
        {
            parts.Add(track.Genre);
        }

        if (!string.IsNullOrWhiteSpace(track.Description))
        {
            parts.Add(track.Description);
        }

        parts.AddRange(track.Tags);
        parts.Add(track.ArtistName);
        return string.Join(" ", parts);
    }

    private class TrackVectorInput
    {
        public string TrackId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private class TrackVector : TrackVectorInput
    {
        public float[]? Features { get; set; }
    }
}
