using Microsoft.Data.Sqlite;
using DroidTidal.Models;
using System.Text.Json;

namespace DroidTidal.Services;

public class LibraryService : IDisposable
{
    private readonly SqliteConnection _db;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public event Action? LibraryChanged;

    public LibraryService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DroidTidal", "library.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _db = new SqliteConnection($"Data Source={dbPath}");
        _db.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS FavoriteTracks (
                Id INTEGER PRIMARY KEY,
                JsonData TEXT NOT NULL,
                AddedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS FavoriteAlbums (
                Id INTEGER PRIMARY KEY,
                Title TEXT,
                Cover TEXT,
                ArtistName TEXT,
                JsonData TEXT NOT NULL,
                AddedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS FavoriteArtists (
                Id INTEGER PRIMARY KEY,
                Name TEXT,
                Picture TEXT,
                JsonData TEXT NOT NULL,
                AddedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS RecentlyPlayed (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TrackId INTEGER NOT NULL,
                JsonData TEXT NOT NULL,
                PlayedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE INDEX IF NOT EXISTS idx_recent_played ON RecentlyPlayed(PlayedAt DESC);
        """;
        cmd.ExecuteNonQuery();
    }

    // ─── Favorite Tracks ─────────────────────────────────────

    public void AddFavoriteTrack(Track track)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO FavoriteTracks (Id, JsonData) VALUES ($id, $json)";
        cmd.Parameters.AddWithValue("$id", track.Id);
        cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(track));
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public void RemoveFavoriteTrack(int trackId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM FavoriteTracks WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", trackId);
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public bool IsFavoriteTrack(int trackId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM FavoriteTracks WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", trackId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public List<Track> GetFavoriteTracks()
    {
        var tracks = new List<Track>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT JsonData FROM FavoriteTracks ORDER BY AddedAt DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var track = JsonSerializer.Deserialize<Track>(reader.GetString(0), _jsonOptions);
            if (track != null) tracks.Add(track);
        }
        return tracks;
    }

    // ─── Favorite Albums ─────────────────────────────────────

    public void AddFavoriteAlbum(AlbumDetail album)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO FavoriteAlbums (Id, Title, Cover, ArtistName, JsonData) VALUES ($id, $title, $cover, $artist, $json)";
        cmd.Parameters.AddWithValue("$id", album.Id);
        cmd.Parameters.AddWithValue("$title", album.Title);
        cmd.Parameters.AddWithValue("$cover", album.Cover ?? "");
        cmd.Parameters.AddWithValue("$artist", album.ArtistName);
        cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(album));
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public void RemoveFavoriteAlbum(int albumId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM FavoriteAlbums WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", albumId);
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public bool IsFavoriteAlbum(int albumId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM FavoriteAlbums WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", albumId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public List<AlbumDetail> GetFavoriteAlbums()
    {
        var albums = new List<AlbumDetail>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT JsonData FROM FavoriteAlbums ORDER BY AddedAt DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var album = JsonSerializer.Deserialize<AlbumDetail>(reader.GetString(0), _jsonOptions);
            if (album != null) albums.Add(album);
        }
        return albums;
    }

    // ─── Favorite Artists ────────────────────────────────────

    public void AddFavoriteArtist(ArtistDetail artist)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO FavoriteArtists (Id, Name, Picture, JsonData) VALUES ($id, $name, $pic, $json)";
        cmd.Parameters.AddWithValue("$id", artist.Id);
        cmd.Parameters.AddWithValue("$name", artist.Name);
        cmd.Parameters.AddWithValue("$pic", artist.Picture ?? "");
        cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(artist));
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public void RemoveFavoriteArtist(int artistId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM FavoriteArtists WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", artistId);
        cmd.ExecuteNonQuery();
        LibraryChanged?.Invoke();
    }

    public List<ArtistDetail> GetFavoriteArtists()
    {
        var artists = new List<ArtistDetail>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT JsonData FROM FavoriteArtists ORDER BY AddedAt DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var artist = JsonSerializer.Deserialize<ArtistDetail>(reader.GetString(0), _jsonOptions);
            if (artist != null) artists.Add(artist);
        }
        return artists;
    }

    // ─── Recently Played ─────────────────────────────────────

    public void AddRecentlyPlayed(Track track)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT INTO RecentlyPlayed (TrackId, JsonData) VALUES ($id, $json)";
        cmd.Parameters.AddWithValue("$id", track.Id);
        cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(track));
        cmd.ExecuteNonQuery();

        // Keep only last 100
        using var cleanup = _db.CreateCommand();
        cleanup.CommandText = """
            DELETE FROM RecentlyPlayed WHERE Id NOT IN (
                SELECT Id FROM RecentlyPlayed ORDER BY PlayedAt DESC LIMIT 100
            )
        """;
        cleanup.ExecuteNonQuery();
    }

    public List<Track> GetRecentlyPlayed(int limit = 50)
    {
        var tracks = new List<Track>();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT JsonData FROM RecentlyPlayed ORDER BY PlayedAt DESC LIMIT $limit";
        cmd.Parameters.AddWithValue("$limit", limit);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var track = JsonSerializer.Deserialize<Track>(reader.GetString(0), _jsonOptions);
            if (track != null) tracks.Add(track);
        }
        return tracks;
    }

    public void Dispose()
    {
        _db?.Dispose();
        GC.SuppressFinalize(this);
    }
}
