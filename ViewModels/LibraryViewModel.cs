using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedTab;

    public ObservableCollection<Track> FavoriteTracks { get; } = [];
    public ObservableCollection<AlbumDetail> FavoriteAlbums { get; } = [];
    public ObservableCollection<ArtistDetail> FavoriteArtists { get; } = [];

    [RelayCommand]
    private void Load()
    {
        LoadTracks();
        LoadAlbums();
        LoadArtists();
    }

    private void LoadTracks()
    {
        FavoriteTracks.Clear();
        foreach (var t in App.LibraryService.GetFavoriteTracks())
            FavoriteTracks.Add(t);
    }

    private void LoadAlbums()
    {
        FavoriteAlbums.Clear();
        foreach (var a in App.LibraryService.GetFavoriteAlbums())
            FavoriteAlbums.Add(a);
    }

    private void LoadArtists()
    {
        FavoriteArtists.Clear();
        foreach (var a in App.LibraryService.GetFavoriteArtists())
            FavoriteArtists.Add(a);
    }

    [RelayCommand]
    private async Task PlayTrack(Track track)
    {
        await App.PlayerService.PlayTrackAsync(track);
    }

    [RelayCommand]
    private async Task PlayAllTracks()
    {
        if (FavoriteTracks.Count > 0)
            await App.PlayerService.PlayTracksAsync(FavoriteTracks.ToList());
    }
}
