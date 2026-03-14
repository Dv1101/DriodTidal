using System.Text.Json.Serialization;

namespace DroidTidal.Models;

// ─── Shared Types ───────────────────────────────────────────

public record ApiResponse<T>(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("data")] T Data
);

public record ArtistRef(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("picture")] string? Picture,
    [property: JsonPropertyName("type")] string? Type
);

public record AlbumRef(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("cover")] string? Cover,
    [property: JsonPropertyName("vibrantColor")] string? VibrantColor,
    [property: JsonPropertyName("videoCover")] string? VideoCover,
    [property: JsonPropertyName("releaseDate")] string? ReleaseDate
);

public record Mixes(
    [property: JsonPropertyName("TRACK_MIX")] string? TrackMix,
    [property: JsonPropertyName("ARTIST_MIX")] string? ArtistMix
);

// ─── Track ──────────────────────────────────────────────────

public record Track(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("trackNumber")] int TrackNumber,
    [property: JsonPropertyName("volumeNumber")] int VolumeNumber,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("popularity")] int Popularity,
    [property: JsonPropertyName("copyright")] string? Copyright,
    [property: JsonPropertyName("explicit")] bool Explicit,
    [property: JsonPropertyName("audioQuality")] string? AudioQuality,
    [property: JsonPropertyName("artist")] ArtistRef? Artist,
    [property: JsonPropertyName("artists")] List<ArtistRef>? Artists,
    [property: JsonPropertyName("album")] AlbumRef? Album,
    [property: JsonPropertyName("mixes")] Mixes? Mixes,
    [property: JsonPropertyName("url")] string? Url
)
{
    public string ArtistName => Artist?.Name ?? (Artists?.FirstOrDefault()?.Name ?? "Unknown");
    public string AlbumTitle => Album?.Title ?? "Unknown Album";
    public string CoverUrl => Album?.Cover != null
        ? $"https://resources.tidal.com/images/{Album.Cover.Replace("-", "/")}/640x640.jpg"
        : "";
    public string SmallCoverUrl => Album?.Cover != null
        ? $"https://resources.tidal.com/images/{Album.Cover.Replace("-", "/")}/160x160.jpg"
        : "";
    public string DurationString
    {
        get
        {
            var ts = TimeSpan.FromSeconds(Duration);
            return ts.Hours > 0 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
        }
    }
    public string QualityBadge => AudioQuality switch
    {
        "HI_RES_LOSSLESS" => "Hi-Res",
        "LOSSLESS" => "CD",
        "HIGH" => "AAC",
        _ => ""
    };
}

// ─── Album ──────────────────────────────────────────────────

public record AlbumDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("numberOfTracks")] int NumberOfTracks,
    [property: JsonPropertyName("numberOfVolumes")] int NumberOfVolumes,
    [property: JsonPropertyName("releaseDate")] string? ReleaseDate,
    [property: JsonPropertyName("copyright")] string? Copyright,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("cover")] string? Cover,
    [property: JsonPropertyName("vibrantColor")] string? VibrantColor,
    [property: JsonPropertyName("explicit")] bool Explicit,
    [property: JsonPropertyName("audioQuality")] string? AudioQuality,
    [property: JsonPropertyName("artist")] ArtistRef? Artist,
    [property: JsonPropertyName("artists")] List<ArtistRef>? Artists,
    [property: JsonPropertyName("items")] List<AlbumItem>? Items
)
{
    public string CoverUrl => Cover != null
        ? $"https://resources.tidal.com/images/{Cover.Replace("-", "/")}/640x640.jpg"
        : "";
    public string LargeCoverUrl => Cover != null
        ? $"https://resources.tidal.com/images/{Cover.Replace("-", "/")}/1280x1280.jpg"
        : "";
    public string ArtistName => Artist?.Name ?? "Unknown";
    public string Year => ReleaseDate?.Split('-').FirstOrDefault() ?? "";
}

public record AlbumItem(
    [property: JsonPropertyName("item")] Track? Item,
    [property: JsonPropertyName("type")] string? Type
);

public record AlbumSearchResult(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("cover")] string? Cover,
    [property: JsonPropertyName("releaseDate")] string? ReleaseDate,
    [property: JsonPropertyName("audioQuality")] string? AudioQuality,
    [property: JsonPropertyName("artist")] ArtistRef? Artist,
    [property: JsonPropertyName("artists")] List<ArtistRef>? Artists,
    [property: JsonPropertyName("numberOfTracks")] int NumberOfTracks
)
{
    public string CoverUrl => Cover != null
        ? $"https://resources.tidal.com/images/{Cover.Replace("-", "/")}/640x640.jpg"
        : "";
    public string ArtistName => Artist?.Name ?? "Unknown";
    public string Year => ReleaseDate?.Split('-').FirstOrDefault() ?? "";
}

// ─── Artist ─────────────────────────────────────────────────

public record ArtistDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("picture")] string? Picture,
    [property: JsonPropertyName("popularity")] int Popularity,
    [property: JsonPropertyName("artistRoles")] List<ArtistRole>? ArtistRoles,
    [property: JsonPropertyName("mixes")] Mixes? Mixes
)
{
    public string ImageUrl => Picture != null
        ? $"https://resources.tidal.com/images/{Picture.Replace("-", "/")}/750x750.jpg"
        : "";
    public List<string> RoleBadges => ArtistRoles?
        .Select(r => r.Category)
        .Where(c => c is not null && c != "Misc")
        .Select(c => c!)
        .Distinct()
        .Take(4)
        .ToList() ?? [];
}

public record ArtistRole(
    [property: JsonPropertyName("categoryId")] int CategoryId,
    [property: JsonPropertyName("category")] string? Category
);

public record ArtistSearchResult(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("picture")] string? Picture,
    [property: JsonPropertyName("popularity")] int Popularity,
    [property: JsonPropertyName("artistRoles")] List<ArtistRole>? ArtistRoles
)
{
    public string ImageUrl => Picture != null
        ? $"https://resources.tidal.com/images/{Picture.Replace("-", "/")}/750x750.jpg"
        : "";
}

public record ArtistReleases(
    [property: JsonPropertyName("albums")] AlbumList? Albums,
    [property: JsonPropertyName("tracks")] List<Track>? Tracks
);

public record AlbumList(
    [property: JsonPropertyName("items")] List<AlbumDetail>? Items
);

// ─── Playlist ───────────────────────────────────────────────

public record PlaylistDetail(
    [property: JsonPropertyName("uuid")] string Uuid,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("numberOfTracks")] int NumberOfTracks,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("squareImage")] string? SquareImage,
    [property: JsonPropertyName("created")] string? Created
)
{
    public string CoverUrl => SquareImage != null
        ? $"https://resources.tidal.com/images/{SquareImage.Replace("-", "/")}/640x640.jpg"
        : "";
}

// ─── Lyrics ─────────────────────────────────────────────────

public record LyricsResponse(
    [property: JsonPropertyName("trackId")] int TrackId,
    [property: JsonPropertyName("lyricsProvider")] string? LyricsProvider,
    [property: JsonPropertyName("lyrics")] string? Lyrics,
    [property: JsonPropertyName("subtitles")] string? Subtitles,
    [property: JsonPropertyName("isRightToLeft")] bool IsRightToLeft
);

public record LyricLine(string Text, TimeSpan Timestamp);

// ─── Stream Manifest ────────────────────────────────────────

public record StreamResponse(
    [property: JsonPropertyName("trackId")] int TrackId,
    [property: JsonPropertyName("audioQuality")] string? AudioQuality,
    [property: JsonPropertyName("manifestMimeType")] string? ManifestMimeType,
    [property: JsonPropertyName("manifest")] string? Manifest,
    [property: JsonPropertyName("bitDepth")] int? BitDepth,
    [property: JsonPropertyName("sampleRate")] int? SampleRate
);

public record DecodedManifest(
    [property: JsonPropertyName("mimeType")] string? MimeType,
    [property: JsonPropertyName("codecs")] string? Codecs,
    [property: JsonPropertyName("urls")] List<string>? Urls
);

// ─── Search ─────────────────────────────────────────────────

public record SearchTrackResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("data")] TrackSearchData? Data
);

public record TrackSearchData(
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("totalNumberOfItems")] int TotalNumberOfItems,
    [property: JsonPropertyName("items")] List<Track>? Items
);

public record SearchArtistResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("data")] ArtistSearchData? Data
);

public record ArtistSearchData(
    [property: JsonPropertyName("artists")] ArtistSearchList? Artists
);

public record ArtistSearchList(
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("totalNumberOfItems")] int TotalNumberOfItems,
    [property: JsonPropertyName("items")] List<ArtistSearchResult>? Items
);

public record SearchAlbumResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("data")] AlbumSearchData? Data
);

public record AlbumSearchData(
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("totalNumberOfItems")] int TotalNumberOfItems,
    [property: JsonPropertyName("items")] List<AlbumSearchResult>? Items
);

// ─── Recommendations ────────────────────────────────────────

public record RecommendationData(
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("totalNumberOfItems")] int TotalNumberOfItems,
    [property: JsonPropertyName("items")] List<RecommendationItem>? Items
);

public record RecommendationItem(
    [property: JsonPropertyName("track")] Track? Track,
    [property: JsonPropertyName("sources")] List<string>? Sources
);

// ─── Mix ────────────────────────────────────────────────────

public record MixResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("mix")] MixInfo? Mix,
    [property: JsonPropertyName("items")] List<Track>? Items
);

public record MixInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("subTitle")] string? SubTitle
);

// ─── Cover Art ──────────────────────────────────────────────

public record CoverArtResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("covers")] List<CoverArt>? Covers
);

public record CoverArt(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("1280")] string? Url1280,
    [property: JsonPropertyName("640")] string? Url640,
    [property: JsonPropertyName("80")] string? Url80
);

// ─── Similar ────────────────────────────────────────────────

public record SimilarArtistsResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("artists")] List<ArtistSearchResult>? Artists
);

public record SimilarAlbumsResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("albums")] List<AlbumSearchResult>? Albums
);

// ─── Artist Info with cover ─────────────────────────────────

public record ArtistInfoResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("artist")] ArtistDetail? Artist,
    [property: JsonPropertyName("cover")] ArtistCover? Cover
);

public record ArtistCover(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("750")] string? Url750
);

public record ArtistReleasesResponse(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("albums")] AlbumList? Albums,
    [property: JsonPropertyName("tracks")] List<Track>? Tracks
);
