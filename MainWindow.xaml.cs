using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using DroidTidal.Views;
using DroidTidal.ViewModels;
using DroidTidal.Services;

namespace DroidTidal;

public sealed partial class MainWindow : Window
{
    private readonly PlayerViewModel _playerVm;
    private bool _isUserSeeking;

    public MainWindow()
    {
        this.InitializeComponent();

        // Enable Mica backdrop
        this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

        // Set title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _playerVm = new PlayerViewModel();
        WirePlayerEvents();

        // Default page
        ContentFrame.Navigate(typeof(HomePage));
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void WirePlayerEvents()
    {
        var player = App.PlayerService;
        var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        player.TrackChanged += () => dispatcher.TryEnqueue(() =>
        {
            var track = player.CurrentTrack;
            if (track != null)
            {
                PlayerTrackTitle.Text = track.Title ?? "Unknown";
                PlayerArtistName.Text = track.ArtistName ?? "";
                if (!string.IsNullOrEmpty(track.CoverUrl))
                {
                    PlayerCoverArt.Source = new BitmapImage(new Uri(track.CoverUrl));
                }
                ProgressSlider.Maximum = track.Duration;
                DurationText.Text = track.DurationString;
            }
        });

        player.StateChanged += () => dispatcher.TryEnqueue(() =>
        {
            PlayPauseIcon.Glyph = player.State == PlaybackState.Playing ? "\uE769" : "\uE768";
        });

        player.PositionUpdated += (pos) => dispatcher.TryEnqueue(() =>
        {
            if (!_isUserSeeking)
            {
                ProgressSlider.Value = pos.TotalSeconds;
                PositionText.Text = pos.Hours > 0 ? pos.ToString(@"h\:mm\:ss") : pos.ToString(@"m\:ss");
            }
        });
    }

    // --- Navigation ---

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.InvokedItemContainer?.Tag is string tag)
        {
            NavigateToPage(tag);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
            ContentFrame.GoBack();
    }

    private void NavigateToPage(string tag)
    {
        Type? pageType = tag switch
        {
            "Home" => typeof(HomePage),
            "Search" => typeof(SearchPage),
            "Library" => typeof(LibraryPage),
            "Queue" => typeof(QueuePage),
            "Lyrics" => typeof(LyricsPage),
            _ => null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    /// <summary>Navigate the content frame to the Artist detail page.</summary>
    public void NavigateToArtist(int artistId)
    {
        ContentFrame.Navigate(typeof(ArtistPage), artistId);
    }

    /// <summary>Navigate the content frame to the Album detail page.</summary>
    public void NavigateToAlbum(int albumId)
    {
        ContentFrame.Navigate(typeof(AlbumPage), albumId);
    }

    // --- Player Controls ---

    private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        => _playerVm.TogglePlayPauseCommand.Execute(null);

    private async void NextBtn_Click(object sender, RoutedEventArgs e)
        => await _playerVm.NextCommand.ExecuteAsync(null);

    private async void PrevBtn_Click(object sender, RoutedEventArgs e)
        => await _playerVm.PreviousCommand.ExecuteAsync(null);

    private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        => _playerVm.ToggleShuffleCommand.Execute(null);

    private void RepeatBtn_Click(object sender, RoutedEventArgs e)
        => _playerVm.ToggleRepeatCommand.Execute(null);

    private void ProgressSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        // Only seek if user is actively dragging
        if (Math.Abs(e.NewValue - e.OldValue) > 1.0)
        {
            App.PlayerService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }
    }

    private void VolumeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        App.PlayerService.Volume = (int)e.NewValue;
    }
}
