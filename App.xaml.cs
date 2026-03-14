using Microsoft.UI.Xaml;
using DroidTidal.Services;

namespace DroidTidal;

public partial class App : Application
{
    public static TidalApiService ApiService { get; private set; } = null!;
    public static AudioPlayerService PlayerService { get; private set; } = null!;
    public static LibraryService LibraryService { get; private set; } = null!;
    public static SettingsService SettingsService { get; private set; } = null!;

    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            SettingsService = new SettingsService();
            ApiService = new TidalApiService(SettingsService);
            PlayerService = new AudioPlayerService(ApiService);
            LibraryService = new LibraryService();

            _window = new MainWindow();
            _window.Activate();
        }
        catch (Exception ex)
        {
            // Fallback for fatal startup errors - at least it won't be completely silent
            System.Diagnostics.Debug.WriteLine($"FATAL STARTUP ERROR: {ex}");
            throw; 
        }
    }

    public static Window? MainAppWindow => ((App)Current)._window;
}
