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
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.WriteAllText(LogPath, "--- STARTUP ---\n");
            WriteLog("OnLaunched started");

            // Initialize LibVLC Core before anything else
            try 
            { 
                LibVLCSharp.Shared.Core.Initialize(); 
                WriteLog("LibVLC Core Initialized");
            } 
            catch (Exception ex) 
            { 
                WriteLog($"LibVLC Core Init FAILED: {ex.Message}"); 
            }

            WriteLog("Initializing Settings");
            SettingsService = new SettingsService();
            
            WriteLog("Initializing API");
            ApiService = new TidalApiService(SettingsService);
            
            WriteLog("Initializing Player");
            PlayerService = new AudioPlayerService(ApiService);
            
            WriteLog("Initializing Library");
            LibraryService = new LibraryService();

            WriteLog("Creating MainWindow");
            _window = new MainWindow();
            
            WriteLog("Activating MainWindow");
            _window.Activate();
            WriteLog("MainWindow Activated");
        }
        catch (Exception ex)
        {
            WriteLog($"FATAL STARTUP ERROR: {ex}");
            System.Diagnostics.Debug.WriteLine($"FATAL STARTUP ERROR: {ex}");
            throw; 
        }
    }

    public static Window? MainAppWindow => ((App)Current)._window;
}
