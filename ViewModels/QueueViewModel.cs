using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using DroidTidal.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class QueueViewModel : ObservableObject
{
    private readonly AudioPlayerService _player;

    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private Track? _currentTrack;

    public ObservableCollection<Track> QueueItems { get; } = [];

    public QueueViewModel()
    {
        _player = App.PlayerService;
        _player.QueueChanged += Refresh;
        _player.TrackChanged += Refresh;
    }

    [RelayCommand]
    private void Load() => Refresh();

    private void Refresh()
    {
        DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
        {
            QueueItems.Clear();
            foreach (var t in _player.Queue) QueueItems.Add(t);
            CurrentIndex = _player.CurrentIndex;
            CurrentTrack = _player.CurrentTrack;
        });
    }

    [RelayCommand]
    private async Task PlayAtIndex(int index)
    {
        if (index >= 0 && index < _player.Queue.Count)
        {
            await _player.PlayTracksAsync(_player.Queue.ToList(), index);
        }
    }

    [RelayCommand]
    private void RemoveFromQueue(int index) => _player.RemoveFromQueue(index);

    [RelayCommand]
    private void ClearQueue() => _player.ClearQueue();
}
