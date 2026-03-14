using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class HomePage : Page
{
    private readonly HomeViewModel _vm = new();

    public HomePage()
    {
        this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await _vm.LoadCommand.ExecuteAsync(null);

        RecentGrid.ItemsSource = _vm.RecentlyPlayed;
        RecsGrid.ItemsSource = _vm.Recommendations;

        EmptyState.Visibility = _vm.RecentlyPlayed.Count == 0 && _vm.Recommendations.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;

        RecsHeader.Visibility = _vm.Recommendations.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void RecentTrack_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await App.PlayerService.PlayTrackAsync(track);
    }

    private async void RecTrack_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
            await App.PlayerService.PlayTrackAsync(track);
    }
}
