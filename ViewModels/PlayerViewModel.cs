using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using DroidTidal.Services;
using Microsoft.UI.Dispatching;

namespace DroidTidal.ViewModels;

public partial class PlayerViewModel : ObservableObject
{
    private readonly AudioPlayerService _player;
    private readonly LibraryService _library;
    private readonly DispatcherQueue _dispatcher;
    private DispatcherQueueTimer? _positionTimer;

    [ObservableProperty] private Track? _currentTrack;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isBuffering;
    [ObservableProperty] private double _position;
    [ObservableProperty] private double _duration;
    [ObservableProperty] private string _positionText = "0:00";
    [ObservableProperty] private string _durationText = "0:00";
    [ObservableProperty] private double _volume = 100;
    [ObservableProperty] private bool _isShuffled;
    [ObservableProperty] private string _repeatIcon = "\uE8EE"; // RepeatAll
    [ObservableProperty] private double _repeatOpacity = 0.5;
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private string _coverUrl = "";

    public PlayerViewModel()
    {
        _player = App.PlayerService;
        _library = App.LibraryService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();

        _player.StateChanged += OnStateChanged;
        _player.TrackChanged += OnTrackChanged;
        _player.PositionUpdated += OnPositionUpdated;

        _positionTimer = _dispatcher.CreateTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(250);
        _positionTimer.Tick += (_, _) => UpdatePosition();
        _positionTimer.Start();
    }

    private void OnStateChanged()
    {
        _dispatcher.TryEnqueue(() =>
        {
            IsPlaying = _player.State == PlaybackState.Playing;
            IsPaused = _player.State == PlaybackState.Paused;
            IsBuffering = _player.State == PlaybackState.Buffering;
            IsShuffled = _player.IsShuffled;
            UpdateRepeatState();
        });
    }

    private void OnTrackChanged()
    {
        _dispatcher.TryEnqueue(() =>
        {
            CurrentTrack = _player.CurrentTrack;
            if (CurrentTrack != null)
            {
                Duration = CurrentTrack.Duration;
                DurationText = CurrentTrack.DurationString;
                CoverUrl = CurrentTrack.CoverUrl;
                IsFavorite = _library.IsFavoriteTrack(CurrentTrack.Id);
                _library.AddRecentlyPlayed(CurrentTrack);
            }
        });
    }

    private void OnPositionUpdated(TimeSpan pos)
    {
        _dispatcher.TryEnqueue(() =>
        {
            Position = pos.TotalSeconds;
            PositionText = pos.Hours > 0 ? pos.ToString(@"h\:mm\:ss") : pos.ToString(@"m\:ss");
        });
    }

    private void UpdatePosition()
    {
        if (_player.State == PlaybackState.Playing)
        {
            var pos = _player.Position;
            Position = pos.TotalSeconds;
            PositionText = pos.Hours > 0 ? pos.ToString(@"h\:mm\:ss") : pos.ToString(@"m\:ss");
        }
    }

    private void UpdateRepeatState()
    {
        switch (_player.Repeat)
        {
            case RepeatMode.Off:
                RepeatIcon = "\uE8EE";
                RepeatOpacity = 0.5;
                break;
            case RepeatMode.All:
                RepeatIcon = "\uE8EE";
                RepeatOpacity = 1.0;
                break;
            case RepeatMode.One:
                RepeatIcon = "\uE8ED";
                RepeatOpacity = 1.0;
                break;
        }
    }

    [RelayCommand]
    private void TogglePlayPause() => _player.TogglePlayPause();

    [RelayCommand]
    private async Task Next() => await _player.NextAsync();

    [RelayCommand]
    private async Task Previous() => await _player.PreviousAsync();

    [RelayCommand]
    private void ToggleShuffle() => _player.ToggleShuffle();

    [RelayCommand]
    private void ToggleRepeat() => _player.CycleRepeatMode();

    [RelayCommand]
    private void SeekToPosition(double seconds) => _player.Seek(TimeSpan.FromSeconds(seconds));

    partial void OnVolumeChanged(double value)
    {
        _player.Volume = (int)value;
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (CurrentTrack == null) return;
        if (IsFavorite)
        {
            _library.RemoveFavoriteTrack(CurrentTrack.Id);
            IsFavorite = false;
        }
        else
        {
            _library.AddFavoriteTrack(CurrentTrack);
            IsFavorite = true;
        }
    }
}
