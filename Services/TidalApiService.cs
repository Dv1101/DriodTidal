using System.Net.Http;
using System.Text.Json;
using DroidTidal.Models;

namespace DroidTidal.Services;

public class TidalApiService
{
    private readonly HttpClient _http;
    private readonly SettingsService _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public TidalApiService(SettingsService settings)
    {
        _settings = settings;
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    private string BaseUrl => _settings.ApiBaseUrl.TrimEnd('/');

    // ─── Search ──────────────────────────────────────────────

    public async Task<List<Track>> SearchTracksAsync(string query, int limit = 25, int offset = 0)
    {
        var url = $"{BaseUrl}/search/?s={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<SearchTrackResponse>(response, _jsonOptions);
        return result?.Data?.Items ?? [];
    }

    public async Task<List<ArtistSearchResult>> SearchArtistsAsync(string query, int limit = 25)
    {
        var url = $"{BaseUrl}/search/?a={Uri.EscapeDataString(query)}&limit={limit}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<SearchArtistResponse>(response, _jsonOptions);
        return result?.Data?.Artists?.Items ?? [];
    }

    public async Task<List<AlbumSearchResult>> SearchAlbumsAsync(string query, int limit = 25)
    {
        var url = $"{BaseUrl}/search/?al={Uri.EscapeDataString(query)}&limit={limit}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<SearchAlbumResponse>(response, _jsonOptions);
        return result?.Data?.Items ?? [];
    }

    // ─── Track ───────────────────────────────────────────────

    public async Task<Track?> GetTrackInfoAsync(int id)
    {
        var url = $"{BaseUrl}/info/?id={id}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<ApiResponse<Track>>(response, _jsonOptions);
        return result?.Data;
    }

    public async Task<StreamResponse?> GetTrackStreamAsync(int id, string? quality = null)
    {
        quality ??= _settings.AudioQuality;
        var url = $"{BaseUrl}/track/?id={id}&quality={quality}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<ApiResponse<StreamResponse>>(response, _jsonOptions);
        return result?.Data;
    }

    public async Task<string?> GetStreamUrlAsync(int trackId, string? quality = null)
    {
        var stream = await GetTrackStreamAsync(trackId, quality);
        if (stream?.Manifest == null) return null;

        if (stream.ManifestMimeType == "application/vnd.tidal.bts")
        {
            var decoded = Convert.FromBase64String(stream.Manifest);
            var json = System.Text.Encoding.UTF8.GetString(decoded);
            var manifest = JsonSerializer.Deserialize<DecodedManifest>(json, _jsonOptions);
            return manifest?.Urls?.FirstOrDefault();
        }

        // For DASH manifests, return null for now (Phase 2)
        return null;
    }

    // ─── Album ───────────────────────────────────────────────

    public async Task<AlbumDetail?> GetAlbumAsync(int id)
    {
        var url = $"{BaseUrl}/album/?id={id}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<ApiResponse<AlbumDetail>>(response, _jsonOptions);
        return result?.Data;
    }

    public async Task<List<AlbumSearchResult>> GetSimilarAlbumsAsync(int id)
    {
        var url = $"{BaseUrl}/album/similar/?id={id}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<SimilarAlbumsResponse>(response, _jsonOptions);
        return result?.Albums ?? [];
    }

    // ─── Artist ──────────────────────────────────────────────

    public async Task<ArtistInfoResponse?> GetArtistAsync(int id)
    {
        var url = $"{BaseUrl}/artist/?id={id}";
        var response = await _http.GetStringAsync(url);
        return JsonSerializer.Deserialize<ArtistInfoResponse>(response, _jsonOptions);
    }

    public async Task<ArtistReleasesResponse?> GetArtistReleasesAsync(int id)
    {
        var url = $"{BaseUrl}/artist/?f={id}";
        var response = await _http.GetStringAsync(url);
        return JsonSerializer.Deserialize<ArtistReleasesResponse>(response, _jsonOptions);
    }

    public async Task<List<ArtistSearchResult>> GetSimilarArtistsAsync(int id)
    {
        var url = $"{BaseUrl}/artist/similar/?id={id}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<SimilarArtistsResponse>(response, _jsonOptions);
        return result?.Artists ?? [];
    }

    // ─── Playlist ────────────────────────────────────────────

    public async Task<(PlaylistDetail? Playlist, List<Track> Tracks)> GetPlaylistAsync(string id)
    {
        var url = $"{BaseUrl}/playlist/?id={id}";
        var response = await _http.GetStringAsync(url);
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;

        PlaylistDetail? playlist = null;
        var tracks = new List<Track>();

        if (root.TryGetProperty("playlist", out var playlistEl))
            playlist = JsonSerializer.Deserialize<PlaylistDetail>(playlistEl.GetRawText(), _jsonOptions);

        if (root.TryGetProperty("items", out var itemsEl))
        {
            foreach (var item in itemsEl.EnumerateArray())
            {
                if (item.TryGetProperty("item", out var trackEl))
                {
                    var track = JsonSerializer.Deserialize<Track>(trackEl.GetRawText(), _jsonOptions);
                    if (track != null) tracks.Add(track);
                }
            }
        }

        return (playlist, tracks);
    }

    // ─── Lyrics ──────────────────────────────────────────────

    public async Task<LyricsResponse?> GetLyricsAsync(int id)
    {
        var url = $"{BaseUrl}/lyrics/?id={id}";
        var response = await _http.GetStringAsync(url);
        using var doc = JsonDocument.Parse(response);
        if (doc.RootElement.TryGetProperty("lyrics", out var lyricsEl))
            return JsonSerializer.Deserialize<LyricsResponse>(lyricsEl.GetRawText(), _jsonOptions);
        return null;
    }

    // ─── Recommendations / Mix ───────────────────────────────

    public async Task<List<Track>> GetRecommendationsAsync(int trackId)
    {
        var url = $"{BaseUrl}/recommendations/?id={trackId}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<ApiResponse<RecommendationData>>(response, _jsonOptions);
        return result?.Data?.Items?
            .Where(i => i.Track != null)
            .Select(i => i.Track!)
            .ToList() ?? [];
    }

    public async Task<MixResponse?> GetMixAsync(string mixId)
    {
        var url = $"{BaseUrl}/mix/?id={mixId}";
        var response = await _http.GetStringAsync(url);
        return JsonSerializer.Deserialize<MixResponse>(response, _jsonOptions);
    }

    // ─── Cover Art ───────────────────────────────────────────

    public async Task<CoverArt?> GetCoverArtAsync(int trackId)
    {
        var url = $"{BaseUrl}/cover/?id={trackId}";
        var response = await _http.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<CoverArtResponse>(response, _jsonOptions);
        return result?.Covers?.FirstOrDefault();
    }
}
