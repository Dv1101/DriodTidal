using DroidTidal.Models;
using LibVLCSharp.Shared;
using System.Text.RegularExpressions;

namespace DroidTidal.Services;

public enum RepeatMode { Off, One, All }
public enum PlaybackState { Stopped, Playing, Paused, Buffering }

public partial class AudioPlayerService : IDisposable
{
    private readonly TidalApiService _api;
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;

    private readonly List<Track> _queue = [];
    private readonly List<Track> _originalQueue = [];
    private int _currentIndex = -1;
    private bool _isShuffled;
    private RepeatMode _repeatMode = RepeatMode.Off;
    private readonly Random _random = new();

    public event Action? StateChanged;
    public event Action? TrackChanged;
    public event Action<TimeSpan>? PositionUpdated;
    public event Action? QueueChanged;

    public Track? CurrentTrack => _currentIndex >= 0 && _currentIndex < _queue.Count
        ? _queue[_currentIndex] : null;

    public PlaybackState State { get; private set; } = PlaybackState.Stopped;
    public TimeSpan Position => _mediaPlayer != null && _mediaPlayer.Length > 0
        ? TimeSpan.FromMilliseconds(_mediaPlayer.Time) : TimeSpan.Zero;
    public TimeSpan Duration => CurrentTrack != null
        ? TimeSpan.FromSeconds(CurrentTrack.Duration) : TimeSpan.Zero;
    public double Progress => Duration.TotalSeconds > 0 ? Position.TotalSeconds / Duration.TotalSeconds : 0;

    public IReadOnlyList<Track> Queue => _queue.AsReadOnly();
    public int CurrentIndex => _currentIndex;
    public bool IsShuffled => _isShuffled;
    public RepeatMode Repeat => _repeatMode;
    public int Volume
    {
        get => _mediaPlayer?.Volume ?? 100;
        set { if (_mediaPlayer != null) _mediaPlayer.Volume = Math.Clamp(value, 0, 100); }
    }

    public AudioPlayerService(TidalApiService api)
    {
        _api = api;
        InitializeVlc();
    }

    private void InitializeVlc()
    {
        try
        {
            Core.Initialize();
            _libVLC = new LibVLC("--no-video");
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += (_, _) =>
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(OnTrackEnded);

            _mediaPlayer.PositionChanged += (_, e) =>
            {
                if (_mediaPlayer.Length > 0)
                {
                    var pos = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
                    PositionUpdated?.Invoke(pos);
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"VLC INIT ERROR: {ex.Message}");
            // We'll set state to Stopped, and future play attempts will fail gracefully
            State = PlaybackState.Stopped;
        }
    }

    // ─── Playback Controls ───────────────────────────────────

    public async Task PlayTrackAsync(Track track)
    {
        _queue.Clear();
        _originalQueue.Clear();
        _queue.Add(track);
        _originalQueue.Add(track);
        _currentIndex = 0;
        _isShuffled = false;
        await PlayCurrentAsync();
    }

    public async Task PlayTracksAsync(List<Track> tracks, int startIndex = 0)
    {
        _queue.Clear();
        _originalQueue.Clear();
        _queue.AddRange(tracks);
        _originalQueue.AddRange(tracks);
        _currentIndex = startIndex;
        _isShuffled = false;
        QueueChanged?.Invoke();
        await PlayCurrentAsync();
    }

    public async Task PlayCurrentAsync()
    {
        if (CurrentTrack == null) return;

        State = PlaybackState.Buffering;
        StateChanged?.Invoke();
        TrackChanged?.Invoke();

        try
        {
            var streamUrl = await _api.GetStreamUrlAsync(CurrentTrack.Id);
            if (streamUrl == null)
            {
                State = PlaybackState.Stopped;
                StateChanged?.Invoke();
                return;
            }

            var media = new Media(_libVLC!, new Uri(streamUrl));
            _mediaPlayer!.Play(media);
            State = PlaybackState.Playing;
        }
        catch
        {
            State = PlaybackState.Stopped;
        }

        StateChanged?.Invoke();
    }

    public void Pause()
    {
        if (_mediaPlayer?.IsPlaying == true)
        {
            _mediaPlayer.Pause();
            State = PlaybackState.Paused;
            StateChanged?.Invoke();
        }
    }

    public void Resume()
    {
        if (State == PlaybackState.Paused)
        {
            _mediaPlayer?.Play();
            State = PlaybackState.Playing;
            StateChanged?.Invoke();
        }
    }

    public void TogglePlayPause()
    {
        if (State == PlaybackState.Playing) Pause();
        else if (State == PlaybackState.Paused) Resume();
    }

    public async Task NextAsync()
    {
        if (_queue.Count == 0) return;

        if (_repeatMode == RepeatMode.One)
        {
            await PlayCurrentAsync();
            return;
        }

        if (_currentIndex < _queue.Count - 1)
        {
            _currentIndex++;
            await PlayCurrentAsync();
        }
        else if (_repeatMode == RepeatMode.All)
        {
            _currentIndex = 0;
            await PlayCurrentAsync();
        }
        else
        {
            Stop();
        }
    }

    public async Task PreviousAsync()
    {
        if (_queue.Count == 0) return;

        if (Position.TotalSeconds > 3)
        {
            Seek(TimeSpan.Zero);
            return;
        }

        if (_currentIndex > 0)
        {
            _currentIndex--;
            await PlayCurrentAsync();
        }
        else if (_repeatMode == RepeatMode.All)
        {
            _currentIndex = _queue.Count - 1;
            await PlayCurrentAsync();
        }
    }

    public void Stop()
    {
        _mediaPlayer?.Stop();
        State = PlaybackState.Stopped;
        StateChanged?.Invoke();
    }

    public void Seek(TimeSpan position)
    {
        if (_mediaPlayer?.Length > 0)
        {
            _mediaPlayer.Time = (long)position.TotalMilliseconds;
        }
    }

    public void SeekPercent(double percent)
    {
        if (_mediaPlayer?.Length > 0)
        {
            _mediaPlayer.Position = (float)Math.Clamp(percent, 0, 1);
        }
    }

    // ─── Queue Management ────────────────────────────────────

    public void AddToQueue(Track track)
    {
        _queue.Add(track);
        _originalQueue.Add(track);
        QueueChanged?.Invoke();
    }

    public void AddToQueueNext(Track track)
    {
        var insertIndex = _currentIndex + 1;
        _queue.Insert(insertIndex, track);
        _originalQueue.Add(track);
        QueueChanged?.Invoke();
    }

    public void RemoveFromQueue(int index)
    {
        if (index < 0 || index >= _queue.Count) return;
        if (index == _currentIndex) return;

        _queue.RemoveAt(index);
        if (index < _currentIndex) _currentIndex--;
        QueueChanged?.Invoke();
    }

    public void ClearQueue()
    {
        var current = CurrentTrack;
        _queue.Clear();
        _originalQueue.Clear();
        if (current != null)
        {
            _queue.Add(current);
            _originalQueue.Add(current);
            _currentIndex = 0;
        }
        QueueChanged?.Invoke();
    }

    // ─── Shuffle & Repeat ────────────────────────────────────

    public void ToggleShuffle()
    {
        _isShuffled = !_isShuffled;
        if (_isShuffled)
        {
            var current = CurrentTrack;
            var remaining = _queue.Where((t, i) => i != _currentIndex).ToList();
            remaining = remaining.OrderBy(_ => _random.Next()).ToList();
            _queue.Clear();
            if (current != null) _queue.Add(current);
            _queue.AddRange(remaining);
            _currentIndex = 0;
        }
        else
        {
            var current = CurrentTrack;
            _queue.Clear();
            _queue.AddRange(_originalQueue);
            _currentIndex = current != null ? _queue.IndexOf(current) : 0;
        }
        QueueChanged?.Invoke();
        StateChanged?.Invoke();
    }

    public void CycleRepeatMode()
    {
        _repeatMode = _repeatMode switch
        {
            RepeatMode.Off => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            RepeatMode.One => RepeatMode.Off,
            _ => RepeatMode.Off
        };
        StateChanged?.Invoke();
    }

    // ─── Event Handlers ──────────────────────────────────────

    private async void OnTrackEnded()
    {
        await NextAsync();
    }

    // ─── Lyrics Parsing ──────────────────────────────────────

    public static List<LyricLine> ParseSyncedLyrics(string subtitles)
    {
        var lines = new List<LyricLine>();
        var regex = TimestampRegex();

        foreach (var line in subtitles.Split('\n'))
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var centiseconds = int.Parse(match.Groups[3].Value);
                var timestamp = new TimeSpan(0, 0, minutes, seconds, centiseconds * 10);
                var text = match.Groups[4].Value.Trim();
                if (!string.IsNullOrEmpty(text))
                    lines.Add(new LyricLine(text, timestamp));
            }
        }

        return lines;
    }

    [GeneratedRegex(@"\[(\d+):(\d+)\.(\d+)\]\s*(.*)")]
    private static partial Regex TimestampRegex();

    // ─── Dispose ─────────────────────────────────────────────

    public void Dispose()
    {
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        GC.SuppressFinalize(this);
    }
}
