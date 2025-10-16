using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MusicPlay.Api.Configuration;
using MusicPlay.Api.DTOs;
using MusicPlay.Api.Infrastructure;
using MusicPlay.Api.Models;
using MusicPlay.Api.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton(sp =>
{
    var options = new StorageOptions();
    builder.Configuration.GetSection("Storage").Bind(options);
    return options;
});

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);
builder.Services.AddSingleton(jwtOptions);

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<TrackService>();
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200 MB
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MusicPlay API",
        Version = "v1",
        Description = "API for the MusicPlay streaming platform focusing on emerging artists."
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var mongoContext = app.Services.GetRequiredService<MongoContext>();
await SeedDataAsync(mongoContext, builder.Configuration);

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("Health");

app.MapPost("/api/auth/register", async (RegisterRequest request, AuthService authService, JwtTokenGenerator tokenGenerator, CancellationToken cancellationToken) =>
{
    var user = await authService.RegisterAsync(request, isAdmin: false, cancellationToken);
    var token = tokenGenerator.GenerateToken(user);
    return Results.Ok(new { Token = token, User = new { user.Id, user.Username, user.Email, user.Roles } });
}).WithName("Register");

app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService, JwtTokenGenerator tokenGenerator, CancellationToken cancellationToken) =>
{
    var user = await authService.LoginAsync(request, cancellationToken);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var token = tokenGenerator.GenerateToken(user);
    return Results.Ok(new { Token = token, User = new { user.Id, user.Username, user.Email, user.Roles } });
}).WithName("Login");

app.MapGet("/api/auth/me", async (ClaimsPrincipal principal, MongoContext context, CancellationToken cancellationToken) =>
{
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(ClaimTypes.Name);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var user = await context.Users.Find(x => x.Id == userId || x.Username == userId).FirstOrDefaultAsync(cancellationToken);
    if (user is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new { user.Id, user.Username, user.Email, user.Roles, user.DisplayName, user.Bio, user.Links });
}).RequireAuthorization().WithName("Me");

app.MapPost("/api/tracks/upload", async (HttpRequest request, TrackUploadRequest metadata, AuthService authService, TrackService trackService, CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Multipart form data is required.");
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest("Audio file is required.");
    }

    var userId = request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
    await using var stream = file.OpenReadStream();
    var track = await trackService.CreateAsync(metadata, stream, file.FileName, userId, cancellationToken);
    return Results.Created($"/api/tracks/{track.Id}", new { track.Id, track.Title });
}).RequireAuthorization().WithName("UploadTrack");

app.MapGet("/api/tracks", async (int page, int pageSize, MongoContext context, TrackService trackService, CancellationToken cancellationToken) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize is <= 0 or > 50 ? 12 : pageSize;

    var total = await context.Tracks.CountDocumentsAsync(FilterDefinition<Track>.Empty, cancellationToken: cancellationToken);
    var tracks = await context.Tracks
        .Find(FilterDefinition<Track>.Empty)
        .SortByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync(cancellationToken);

    var payload = tracks.Select(track => new TrackResponse(
        track.Id,
        track.Title,
        track.ArtistName,
        track.Description,
        track.Genre,
        track.Tags,
        track.Duration,
        trackService.BuildStreamUrl(track),
        track.CoverUrl,
        track.Featured,
        track.PlayCount,
        track.CreatedAt
    ));

    return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = payload });
}).WithName("ListTracks");

app.MapGet("/api/tracks/featured", async (TrackService trackService, CancellationToken cancellationToken) =>
{
    var tracks = await trackService.GetFeaturedAsync(6, cancellationToken);
    return Results.Ok(tracks.Select(track => new TrackResponse(
        track.Id,
        track.Title,
        track.ArtistName,
        track.Description,
        track.Genre,
        track.Tags,
        track.Duration,
        trackService.BuildStreamUrl(track),
        track.CoverUrl,
        track.Featured,
        track.PlayCount,
        track.CreatedAt
    )));
}).WithName("FeaturedTracks");

app.MapGet("/api/tracks/{id}/stream", async (string id, HttpRequest request, HttpResponse response, TrackService trackService, CancellationToken cancellationToken) =>
{
    var track = await trackService.GetByIdAsync(id, cancellationToken);
    if (track is null)
    {
        return Results.NotFound();
    }

    var fileInfo = new FileInfo(track.FilePath);
    if (!fileInfo.Exists)
    {
        return Results.NotFound();
    }

    if (!trackService.TryGetContentType(fileInfo.Name, out var contentType))
    {
        contentType = "audio/mpeg";
    }

    var stream = trackService.OpenRead(fileInfo.FullName);
    var rangeHeader = request.Headers.Range.ToString();

    if (string.IsNullOrEmpty(rangeHeader))
    {
        response.Headers.ContentType = contentType;
        return Results.File(stream, contentType, enableRangeProcessing: true);
    }

    var range = rangeHeader.Replace("bytes=", string.Empty).Split('-');
    if (!long.TryParse(range[0], out var start))
    {
        start = 0;
    }

    var end = range.Length > 1 && long.TryParse(range[1], out var parsedEnd) ? parsedEnd : fileInfo.Length - 1;
    var length = end - start + 1;

    stream.Seek(start, SeekOrigin.Begin);
    response.StatusCode = (int)HttpStatusCode.PartialContent;
    response.Headers.ContentType = contentType;
    response.Headers.ContentLength = length;
    response.Headers.AcceptRanges = "bytes";
    response.Headers.ContentRange = $"bytes {start}-{end}/{fileInfo.Length}";

    return Results.File(stream, contentType, enableRangeProcessing: true);
}).WithName("StreamTrack");

app.MapGet("/api/playlists/featured", async (MongoContext context, TrackService trackService, CancellationToken cancellationToken) =>
{
    var playlists = await context.Playlists
        .Find(p => p.IsPublic)
        .SortByDescending(p => p.UpdatedAt)
        .Limit(4)
        .ToListAsync(cancellationToken);

    var trackDictionary = (await context.Tracks
        .Find(FilterDefinition<Track>.Empty)
        .ToListAsync(cancellationToken))
        .ToDictionary(t => t.Id, t => t);

    var payload = playlists.Select(playlist => new PlaylistResponse(
        playlist.Id,
        playlist.Title,
        playlist.Description,
        playlist.TrackIds
            .Where(trackDictionary.ContainsKey)
            .Select(id => trackDictionary[id])
            .Select(track => new TrackResponse(
                track.Id,
                track.Title,
                track.ArtistName,
                track.Description,
                track.Genre,
                track.Tags,
                track.Duration,
                trackService.BuildStreamUrl(track),
                track.CoverUrl,
                track.Featured,
                track.PlayCount,
                track.CreatedAt
            ))
            .ToList(),
        playlist.CoverUrl,
        playlist.IsPublic
    ));

    return Results.Ok(payload);
}).WithName("FeaturedPlaylists");

app.MapGet("/api/seo/sitemap", async (MongoContext context, TrackService trackService, CancellationToken cancellationToken) =>
{
    var baseUrl = "https://musicplay.local";
    var tracks = await context.Tracks.Find(FilterDefinition<Track>.Empty).ToListAsync(cancellationToken);
    var urls = new List<object>
    {
        new { loc = baseUrl, changefreq = "daily", priority = 1.0 },
        new { loc = $"{baseUrl}/discover", changefreq = "daily", priority = 0.9 }
    };

    urls.AddRange(tracks.Select(track => new
    {
        loc = $"{baseUrl}/tracks/{track.Id}",
        changefreq = "weekly",
        priority = 0.7
    }));

    return Results.Ok(urls);
}).WithName("Sitemap");

app.Run();

static async Task SeedDataAsync(MongoContext context, IConfiguration configuration)
{
    var adminHash = configuration.GetValue<string>("Admin:SeedPasswordHash") ?? "";
    var userHash = configuration.GetValue<string>("User:SeedPasswordHash") ?? "";

    var usersCollection = context.Users;
    if (!await usersCollection.Find(u => u.Username == "admin").AnyAsync())
    {
        await usersCollection.InsertOneAsync(new ApplicationUser
        {
            Username = "admin",
            Email = "admin@musicplay.local",
            PasswordHash = adminHash,
            Roles = new List<string> { "Admin", "User" },
            DisplayName = "MusicPlay Admin"
        });
    }

    if (!await usersCollection.Find(u => u.Username == "user").AnyAsync())
    {
        await usersCollection.InsertOneAsync(new ApplicationUser
        {
            Username = "user",
            Email = "user@musicplay.local",
            PasswordHash = userHash,
            Roles = new List<string> { "User" },
            DisplayName = "MusicPlay Fan"
        });
    }

    if (!await context.Playlists.Find(p => p.Title == "New Voices Spotlight").AnyAsync())
    {
        await context.Playlists.InsertOneAsync(new Playlist
        {
            Title = "New Voices Spotlight",
            Description = "A curated list of tracks from emerging independent artists.",
            TrackIds = new List<string>(),
            CoverUrl = "/assets/playlists/new-voices.jpg",
            OwnerId = "admin",
            IsPublic = true
        });
    }
}
