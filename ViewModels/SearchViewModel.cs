using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty] private string _queryText = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _selectedTab;
    [ObservableProperty] private string _resultSummary = "";

    public ObservableCollection<Track> Tracks { get; } = [];
    public ObservableCollection<ArtistSearchResult> Artists { get; } = [];
    public ObservableCollection<AlbumSearchResult> Albums { get; } = [];

    private CancellationTokenSource? _debounce;

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(QueryText)) return;

        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;

        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            IsLoading = true;

            var trackTask = App.ApiService.SearchTracksAsync(QueryText);
            var artistTask = App.ApiService.SearchArtistsAsync(QueryText);
            var albumTask = App.ApiService.SearchAlbumsAsync(QueryText);

            await Task.WhenAll(trackTask, artistTask, albumTask);

            if (token.IsCancellationRequested) return;

            Tracks.Clear();
            foreach (var t in await trackTask) Tracks.Add(t);

            Artists.Clear();
            foreach (var a in await artistTask) Artists.Add(a);

            Albums.Clear();
            foreach (var a in await albumTask) Albums.Add(a);

            ResultSummary = $"{Tracks.Count} tracks, {Artists.Count} artists, {Albums.Count} albums";
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            ResultSummary = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlayTrack(Track track)
    {
        await App.PlayerService.PlayTrackAsync(track);
    }

    [RelayCommand]
    private async Task PlayAllTracks()
    {
        if (Tracks.Count > 0)
            await App.PlayerService.PlayTracksAsync(Tracks.ToList());
    }
}
