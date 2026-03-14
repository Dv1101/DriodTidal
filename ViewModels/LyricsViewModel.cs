using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using DroidTidal.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class LyricsViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasSyncedLyrics;
    [ObservableProperty] private string _plainLyrics = "";
    [ObservableProperty] private int _currentLineIndex = -1;
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<LyricLine> SyncedLines { get; } = [];

    private readonly AudioPlayerService _player;
    private int _lastTrackId;

    public LyricsViewModel()
    {
        _player = App.PlayerService;
        _player.TrackChanged += OnTrackChanged;
        _player.PositionUpdated += OnPositionUpdated;
    }

    private async void OnTrackChanged()
    {
        var track = _player.CurrentTrack;
        if (track == null || track.Id == _lastTrackId) return;
        _lastTrackId = track.Id;
        await LoadLyricsAsync(track.Id);
    }

    [RelayCommand]
    private async Task LoadLyricsAsync(int trackId)
    {
        IsLoading = true;
        ErrorMessage = "";
        HasSyncedLyrics = false;
        PlainLyrics = "";
        SyncedLines.Clear();
        CurrentLineIndex = -1;

        try
        {
            var lyrics = await App.ApiService.GetLyricsAsync(trackId);
            if (lyrics == null)
            {
                ErrorMessage = "No lyrics available";
                return;
            }

            if (!string.IsNullOrWhiteSpace(lyrics.Subtitles))
            {
                var lines = AudioPlayerService.ParseSyncedLyrics(lyrics.Subtitles);
                foreach (var line in lines) SyncedLines.Add(line);
                HasSyncedLyrics = true;
            }
            else if (!string.IsNullOrWhiteSpace(lyrics.Lyrics))
            {
                PlainLyrics = lyrics.Lyrics;
            }
            else
            {
                ErrorMessage = "No lyrics available";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load lyrics: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnPositionUpdated(TimeSpan position)
    {
        if (!HasSyncedLyrics || SyncedLines.Count == 0) return;

        var newIndex = -1;
        for (int i = SyncedLines.Count - 1; i >= 0; i--)
        {
            if (position >= SyncedLines[i].Timestamp)
            {
                newIndex = i;
                break;
            }
        }

        if (newIndex != CurrentLineIndex)
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                CurrentLineIndex = newIndex;
            });
        }
    }
}
