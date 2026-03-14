using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class LibraryPage : Page
{
    private readonly LibraryViewModel _vm = new();

    public LibraryPage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _vm.LoadCommand.Execute(null);
        BindLibrary();

        App.LibraryService.LibraryChanged += () => DispatcherQueue.TryEnqueue(() =>
        {
            _vm.LoadCommand.Execute(null);
            BindLibrary();
        });
    }

    private void BindLibrary()
    {
        FavTracksList.ItemsSource = _vm.FavoriteTracks;
        FavAlbumsGrid.ItemsSource = _vm.FavoriteAlbums;
        FavArtistsGrid.ItemsSource = _vm.FavoriteArtists;

        PlayAllBtn.Visibility = _vm.FavoriteTracks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyTracks.Visibility = _vm.FavoriteTracks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void FavTrack_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await _vm.PlayTrackCommand.ExecuteAsync(track);
    }

    private async void PlayAllTracks_Click(object sender, RoutedEventArgs e)
        => await _vm.PlayAllTracksCommand.ExecuteAsync(null);

    private void FavAlbum_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AlbumDetail album && App.MainAppWindow is MainWindow mw)
            mw.NavigateToAlbum(album.Id);
    }

    private void FavArtist_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ArtistDetail artist && App.MainAppWindow is MainWindow mw)
            mw.NavigateToArtist(artist.Id);
    }
}
