using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System;
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

            Stream log_file = File.Create($"C:/Users/Karsten/Downloads/log-{DateTime.Now.ToString("s").Replace(':', '-')}.txt");
            Trace.Listeners.Add(new TextWriterTraceListener(log_file));
            Trace.AutoFlush = true;
            Trace.Indent();

            Trace.WriteLine(string.Join('\n', args));

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
