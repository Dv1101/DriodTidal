using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidTidal.Models;
using System.Collections.ObjectModel;

namespace DroidTidal.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<Track> RecentlyPlayed { get; } = [];
    public ObservableCollection<Track> Recommendations { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            // Load recently played from local DB
            var recent = App.LibraryService.GetRecentlyPlayed(20);
            RecentlyPlayed.Clear();
            foreach (var t in recent) RecentlyPlayed.Add(t);

            // If we have a recently played track, get recommendations from it
            if (recent.Count > 0)
            {
                try
                {
                    var recs = await App.ApiService.GetRecommendationsAsync(recent.First().Id);
                    Recommendations.Clear();
                    foreach (var t in recs.Take(20)) Recommendations.Add(t);
                }
                catch { /* API may not have recommendations */ }
            }
        }
        catch { }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlayTrack(Track track)
    {
        await App.PlayerService.PlayTrackAsync(track);
    }
}
