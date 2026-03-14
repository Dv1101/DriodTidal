using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class AlbumViewModel : ObservableObject
{
    [ObservableProperty] private AlbumDetail? _album;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<Track> Tracks { get; } = [];
    public ObservableCollection<AlbumSearchResult> SimilarAlbums { get; } = [];

    [RelayCommand]
    private async Task LoadAlbumAsync(int albumId)
    {
        IsLoading = true;
        ErrorMessage = "";
        try
        {
            Album = await App.ApiService.GetAlbumAsync(albumId);
            if (Album?.Items != null)
            {
                Tracks.Clear();
                foreach (var item in Album.Items)
                {
                    if (item.Item != null) Tracks.Add(item.Item);
                }
            }

            IsFavorite = App.LibraryService.IsFavoriteAlbum(albumId);

            // Load similar albums in background
            _ = LoadSimilarAsync(albumId);
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

    private async Task LoadSimilarAsync(int albumId)
    {
        try
        {
            var similar = await App.ApiService.GetSimilarAlbumsAsync(albumId);
            SimilarAlbums.Clear();
            foreach (var a in similar.Take(10)) SimilarAlbums.Add(a);
        }
        catch { /* non-critical */ }
    }

    [RelayCommand]
    private async Task PlayTrack(Track track)
    {
        var index = Tracks.IndexOf(track);
        await App.PlayerService.PlayTracksAsync(Tracks.ToList(), Math.Max(0, index));
    }

    [RelayCommand]
    private async Task PlayAll()
    {
        if (Tracks.Count > 0)
            await App.PlayerService.PlayTracksAsync(Tracks.ToList());
    }

    [RelayCommand]
    private async Task ShufflePlay()
    {
        if (Tracks.Count > 0)
        {
            await App.PlayerService.PlayTracksAsync(Tracks.ToList());
            App.PlayerService.ToggleShuffle();
        }
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Album == null) return;
        if (IsFavorite)
        {
            App.LibraryService.RemoveFavoriteAlbum(Album.Id);
            IsFavorite = false;
        }
        else
        {
            App.LibraryService.AddFavoriteAlbum(Album);
            IsFavorite = true;
        }
    }

    [RelayCommand]
    private void AddToQueue(Track track)
    {
        App.PlayerService.AddToQueue(track);
    }

    [RelayCommand]
    private void PlayNext(Track track)
    {
        App.PlayerService.AddToQueueNext(track);
    }
}
