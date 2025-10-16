namespace MusicPlay.Api.Configuration;

public class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string BucketName { get; set; } = "audio-files";
}
