using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DroidTidal.Models;

namespace DroidTidal.Views;

public sealed partial class QueuePage : Page
{
    public QueuePage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshQueue();
        App.PlayerService.QueueChanged += () => DispatcherQueue.TryEnqueue(RefreshQueue);
        App.PlayerService.TrackChanged += () => DispatcherQueue.TryEnqueue(RefreshQueue);
    }

    private void RefreshQueue()
    {
        var queue = App.PlayerService.Queue;
        QueueList.ItemsSource = queue;
        QueueCount.Text = $"{queue.Count} tracks in queue";

        EmptyState.Visibility = queue.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ClearBtn.Visibility = queue.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void QueueItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Track track)
        {
            var index = App.PlayerService.Queue.ToList().IndexOf(track);
            if (index >= 0)
                await App.PlayerService.PlayTracksAsync(App.PlayerService.Queue.ToList(), index);
        }
    }

    private void ClearQueue_Click(object sender, RoutedEventArgs e)
    {
        App.PlayerService.ClearQueue();
    }
}
