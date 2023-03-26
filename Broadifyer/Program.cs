using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;

namespace Broadifyer
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Environment.CurrentDirectory);

            Directory.CreateDirectory("logs");

            Stream log_file = File.Create($"logs/log-{DateTime.Now.ToString("s").Replace(':', '-')}.txt");
            Trace.Listeners.Add(new TextWriterTraceListener(log_file));
            Trace.AutoFlush = true;
            Trace.Indent();

            Trace.WriteLine(string.Join('\n', args));
#if !DEBUG
            try
            {
#endif
                BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
#if !DEBUG
        }
            catch(Exception e)
            {
                Trace.WriteLine(e);
                Trace.Flush();
                throw;
            }
#endif
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions { UseWindowsUIComposition = false })
                .LogToTrace()
                .UseReactiveUI();
    }
}
