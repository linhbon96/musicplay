using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MusicPlay.Api.Configuration;
using MusicPlay.Api.Models;

namespace MusicPlay.Api.Infrastructure;

public class MongoContext
{
    public IMongoDatabase Database { get; }

    public IMongoCollection<ApplicationUser> Users => Database.GetCollection<ApplicationUser>("users");
    public IMongoCollection<Track> Tracks => Database.GetCollection<Track>("tracks");
    public IMongoCollection<Playlist> Playlists => Database.GetCollection<Playlist>("playlists");

    public MongoContext(IOptions<MongoOptions> options)
    {
        var settings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
        settings.LinqProvider = LinqProvider.V3;
        var client = new MongoClient(settings);
        Database = client.GetDatabase(options.Value.DatabaseName);
    }
}
