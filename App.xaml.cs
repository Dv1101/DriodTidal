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
    private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DroidTidal", "startup.log");

    private void WriteLog(string msg)
    {
        try { File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {msg}\n"); } catch { }
    }

    public App()
    {
        this.InitializeComponent();

        // Initialize LibVLC Core early, like Droid Music
        try 
        { 
            LibVLCSharp.Shared.Core.Initialize(); 
        } 
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine($"LibVLC Core Init FAILED: {ex.Message}"); 
        }

        SettingsService = new SettingsService();
        ApiService = new TidalApiService(SettingsService);
        PlayerService = new AudioPlayerService(ApiService);
        LibraryService = new LibraryService();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _window = new MainWindow();
            _window.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FATAL STARTUP ERROR: {ex}");
            throw; 
        }
    }

    public static Window? MainAppWindow => ((App)Current)._window;
}
