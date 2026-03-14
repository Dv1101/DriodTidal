using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DroidTidal.Models;
using DroidTidal.ViewModels;

namespace DroidTidal.Views;

public sealed partial class LyricsPage : Page
{
    private readonly LyricsViewModel _vm = new();

    public LyricsPage()
    {
        this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var track = App.PlayerService.CurrentTrack;
        if (track == null)
        {
            ShowError("Play a track to see lyrics");
            return;
        }

        TrackInfo.Text = $"{track.Title} • {track.ArtistName}";
        LoadingRing.IsActive = true;

        await _vm.LoadLyricsCommand.ExecuteAsync(track.Id);

        LoadingRing.IsActive = false;

        if (_vm.HasSyncedLyrics)
        {
            SyncedList.ItemsSource = _vm.SyncedLines;
            SyncedList.Visibility = Visibility.Visible;
            _vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(_vm.CurrentLineIndex))
                    HighlightCurrentLine();
            };
        }
        else if (!string.IsNullOrEmpty(_vm.PlainLyrics))
        {
            PlainLyricsText.Text = _vm.PlainLyrics;
            PlainLyricsScroll.Visibility = Visibility.Visible;
        }
        else
        {
            ShowError(_vm.ErrorMessage ?? "No lyrics available");
        }

        // Listen for track changes
        App.PlayerService.TrackChanged += OnTrackChanged;
    }

    private async void OnTrackChanged()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            var track = App.PlayerService.CurrentTrack;
            if (track == null) return;

            TrackInfo.Text = $"{track.Title} • {track.ArtistName}";
            SyncedList.Visibility = Visibility.Collapsed;
            PlainLyricsScroll.Visibility = Visibility.Collapsed;
            ErrorState.Visibility = Visibility.Collapsed;
            LoadingRing.IsActive = true;

            await _vm.LoadLyricsCommand.ExecuteAsync(track.Id);

            LoadingRing.IsActive = false;

            if (_vm.HasSyncedLyrics)
            {
                SyncedList.ItemsSource = _vm.SyncedLines;
                SyncedList.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(_vm.PlainLyrics))
            {
                PlainLyricsText.Text = _vm.PlainLyrics;
                PlainLyricsScroll.Visibility = Visibility.Visible;
            }
            else
            {
                ShowError(_vm.ErrorMessage ?? "No lyrics available");
            }
        });
    }

    private void HighlightCurrentLine()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var idx = _vm.CurrentLineIndex;
            if (idx >= 0 && idx < _vm.SyncedLines.Count)
            {
                // Scroll to current line
                SyncedList.ScrollIntoView(_vm.SyncedLines[idx], ScrollIntoViewAlignment.Leading);

                // Update opacity for karaoke effect
                for (int i = 0; i < SyncedList.Items.Count; i++)
                {
                    if (SyncedList.ContainerFromIndex(i) is ListViewItem item)
                    {
                        item.Opacity = i == idx ? 1.0 : 0.4;
                    }
                }
            }
        });
    }

    private void ShowError(string msg)
    {
        ErrorState.Visibility = Visibility.Visible;
        ErrorText.Text = msg;
    }
}
