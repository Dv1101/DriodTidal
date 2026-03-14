using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace DroidTidal;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // For unpackaged apps, we must initialize the Windows App SDK bootstrapper.
        try
        {
            // Initialize the bootstrapper for 1.8
            Bootstrap.Initialize(0x00010008, "1.8"); 
            
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
        catch (Exception ex)
        {
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DroidTidal", "fatal_crash.log");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
            System.IO.File.WriteAllText(logPath, $"BOOTSTRAP FAILURE: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
        finally
        {
            Bootstrap.Shutdown();
        }
    }
}
