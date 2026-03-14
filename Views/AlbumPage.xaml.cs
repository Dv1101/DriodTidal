using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class AlbumPage : Page
{
    private readonly AlbumViewModel _vm = new();

    public AlbumPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int albumId)
        {
            LoadingRing.IsActive = true;
            await _vm.LoadAlbumCommand.ExecuteAsync(albumId);
            LoadingRing.IsActive = false;
            BindAlbum();
        }
    }

    private void BindAlbum()
    {
        if (_vm.Album == null) return;

        AlbumHeader.Visibility = Visibility.Visible;
        AlbumTitle.Text = _vm.Album.Title;
        AlbumArtist.Content = _vm.Album.ArtistName;
        AlbumType.Text = (_vm.Album.Type ?? "ALBUM").ToUpperInvariant();
        AlbumMeta.Text = $"{_vm.Album.Year} • {_vm.Album.NumberOfTracks} tracks";

        if (!string.IsNullOrEmpty(_vm.Album.CoverUrl))
            AlbumCover.ImageSource = new BitmapImage(new Uri(_vm.Album.CoverUrl));

        FavoriteIcon.Glyph = _vm.IsFavorite ? "\uEB52" : "\uEB51";

        TrackList.ItemsSource = _vm.Tracks;

        if (_vm.SimilarAlbums.Count > 0)
        {
            SimilarHeader.Visibility = Visibility.Visible;
            SimilarGrid.ItemsSource = _vm.SimilarAlbums;
        }
    }

    private async void TrackItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await _vm.PlayTrackCommand.ExecuteAsync(track);
    }

    private async void PlayAll_Click(object sender, RoutedEventArgs e)
        => await _vm.PlayAllCommand.ExecuteAsync(null);

    private async void ShufflePlay_Click(object sender, RoutedEventArgs e)
        => await _vm.ShufflePlayCommand.ExecuteAsync(null);

    private void Favorite_Click(object sender, RoutedEventArgs e)
    {
        _vm.ToggleFavoriteCommand.Execute(null);
        FavoriteIcon.Glyph = _vm.IsFavorite ? "\uEB52" : "\uEB51";
    }

    private void ArtistLink_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.Album?.Artist?.Id is int id && App.MainAppWindow is MainWindow mw)
            mw.NavigateToArtist(id);
    }

    private void SimilarAlbum_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AlbumSearchResult album && App.MainAppWindow is MainWindow mw)
            mw.NavigateToAlbum(album.Id);
    }
}
