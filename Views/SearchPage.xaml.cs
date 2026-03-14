using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class SearchPage : Page
{
    private readonly SearchViewModel _vm = new();

    public SearchPage()
    {
        this.InitializeComponent();
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        _vm.QueryText = args.QueryText;
        await PerformSearch();
    }

    private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length >= 2)
        {
            _vm.QueryText = sender.Text;
            await PerformSearch();
        }
    }

    private async Task PerformSearch()
    {
        LoadingRing.IsActive = true;
        await _vm.SearchCommand.ExecuteAsync(null);
        LoadingRing.IsActive = false;

        TracksList.ItemsSource = _vm.Tracks;
        ArtistGrid.ItemsSource = _vm.Artists;
        AlbumGrid.ItemsSource = _vm.Albums;
    }

    private async void Track_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await _vm.PlayTrackCommand.ExecuteAsync(track);
    }

    private void Artist_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ArtistSearchResult artist)
        {
            if (App.MainAppWindow is MainWindow mw)
                mw.NavigateToArtist(artist.Id);
        }
    }

    private void Album_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AlbumSearchResult album)
        {
            if (App.MainAppWindow is MainWindow mw)
                mw.NavigateToAlbum(album.Id);
        }
    }
}
