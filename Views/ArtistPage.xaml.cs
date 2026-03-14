using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class ArtistPage : Page
{
    private readonly ArtistViewModel _vm = new();

    public ArtistPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is int artistId)
        {
            LoadingRing.IsActive = true;
            await _vm.LoadArtistCommand.ExecuteAsync(artistId);
            LoadingRing.IsActive = false;
            BindArtist();

            // Wait for releases to load then rebind
            _vm.TopTracks.CollectionChanged += (_, _) =>
                DispatcherQueue.TryEnqueue(BindReleases);
            _vm.Albums.CollectionChanged += (_, _) =>
                DispatcherQueue.TryEnqueue(BindReleases);
            _vm.SimilarArtists.CollectionChanged += (_, _) =>
                DispatcherQueue.TryEnqueue(BindSimilar);
        }
    }

    private void BindArtist()
    {
        if (_vm.Artist == null) return;

        ArtistHeader.Visibility = Visibility.Visible;
        ArtistName.Text = _vm.Artist.Name;

        if (!string.IsNullOrEmpty(_vm.ArtistImageUrl))
            ArtistImage.Fill = new Microsoft.UI.Xaml.Media.ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(_vm.ArtistImageUrl)),
                Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill
            };

        RoleBadges.Children.Clear();
        foreach (var role in _vm.RoleBadges)
        {
            var badge = new Border
            {
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                Child = new TextBlock
                {
                    Text = role,
                    FontSize = 11,
                    Opacity = 0.8
                }
            };
            RoleBadges.Children.Add(badge);
        }
    }

    private void BindReleases()
    {
        if (_vm.TopTracks.Count > 0)
        {
            TopTracksHeader.Visibility = Visibility.Visible;
            TopTracksList.ItemsSource = _vm.TopTracks;
        }
        if (_vm.Albums.Count > 0)
        {
            AlbumsHeader.Visibility = Visibility.Visible;
            AlbumsGrid.ItemsSource = _vm.Albums;
        }
    }

    private void BindSimilar()
    {
        if (_vm.SimilarArtists.Count > 0)
        {
            SimilarHeader.Visibility = Visibility.Visible;
            SimilarGrid.ItemsSource = _vm.SimilarArtists;
        }
    }

    private async void TopTrack_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await _vm.PlayTrackCommand.ExecuteAsync(track);
    }

    private async void ShuffleAll_Click(object sender, RoutedEventArgs e)
        => await _vm.ShuffleAllCommand.ExecuteAsync(null);

    private void AlbumItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AlbumDetail album && App.MainAppWindow is MainWindow mw)
            mw.NavigateToAlbum(album.Id);
    }

    private void SimilarArtist_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ArtistSearchResult artist && App.MainAppWindow is MainWindow mw)
            mw.NavigateToArtist(artist.Id);
    }
}
