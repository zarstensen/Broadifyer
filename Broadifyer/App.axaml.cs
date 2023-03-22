using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using Broadifyer.ViewModels;
using Broadifyer.Views;

namespace Broadifyer
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                

                desktop.Startup += (sender, args) =>
                    {
                        (desktop.MainWindow as MainWindow)!.start_minimized = args.Args.Contains("--minimized");
                    };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
