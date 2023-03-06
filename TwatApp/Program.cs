using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;

namespace TwatApp
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Directory.CreateDirectory("logs");

            Stream log_file = File.Create($"logs/log-{DateTime.Now.ToString("s").Replace(':', '-')}.txt");
            Trace.Listeners.Add(new TextWriterTraceListener(log_file));
            Trace.AutoFlush = true;
            Trace.Indent();

            Trace.WriteLine(string.Join('\n', args));

            try
            {
                BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            }
            catch(Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
