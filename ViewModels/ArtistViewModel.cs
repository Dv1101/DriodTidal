using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class ArtistViewModel : ObservableObject
{
    [ObservableProperty] private ArtistDetail? _artist;
    [ObservableProperty] private string _artistImageUrl = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<string> RoleBadges { get; } = [];
    public ObservableCollection<Track> TopTracks { get; } = [];
    public ObservableCollection<AlbumDetail> Albums { get; } = [];
    public ObservableCollection<ArtistSearchResult> SimilarArtists { get; } = [];

    [RelayCommand]
    private async Task LoadArtistAsync(int artistId)
    {
        IsLoading = true;
        ErrorMessage = "";
        try
        {
            var infoResponse = await App.ApiService.GetArtistAsync(artistId);
            Artist = infoResponse?.Artist;
            ArtistImageUrl = infoResponse?.Cover?.Url750 ?? Artist?.ImageUrl ?? "";

            RoleBadges.Clear();
            if (Artist?.RoleBadges != null)
                foreach (var r in Artist.RoleBadges) RoleBadges.Add(r);

            // Load releases (slower endpoint)
            _ = LoadReleasesAsync(artistId);
            _ = LoadSimilarAsync(artistId);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadReleasesAsync(int artistId)
    {
        try
        {
            var releases = await App.ApiService.GetArtistReleasesAsync(artistId);
            if (releases?.Tracks != null)
            {
                TopTracks.Clear();
                foreach (var t in releases.Tracks.Take(10)) TopTracks.Add(t);
            }
            if (releases?.Albums?.Items != null)
            {
                Albums.Clear();
                foreach (var a in releases.Albums.Items) Albums.Add(a);
            }
        }
        catch { /* non-critical */ }
    }

    private async Task LoadSimilarAsync(int artistId)
    {
        try
        {
            var similar = await App.ApiService.GetSimilarArtistsAsync(artistId);
            SimilarArtists.Clear();
            foreach (var a in similar.Take(10)) SimilarArtists.Add(a);
        }
        catch { /* non-critical */ }
    }

    [RelayCommand]
    private async Task PlayTrack(Track track)
    {
        var index = TopTracks.IndexOf(track);
        await App.PlayerService.PlayTracksAsync(TopTracks.ToList(), Math.Max(0, index));
    }

    [RelayCommand]
    private async Task ShuffleAll()
    {
        if (TopTracks.Count > 0)
        {
            await App.PlayerService.PlayTracksAsync(TopTracks.ToList());
            App.PlayerService.ToggleShuffle();
        }
    }
}
