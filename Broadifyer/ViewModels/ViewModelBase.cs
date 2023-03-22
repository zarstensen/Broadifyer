using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Broadifyer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public AppViewModel AppVM { get => (App.Current!.DataContext as AppViewModel)!; }
        public MainWindowViewModel WindowVM { get
            {
                if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    return (MainWindowViewModel)desktop.MainWindow.DataContext!;
                else
                    throw new ArgumentException("desktop.MainWindow should have a datacontext of type MainWindowViewModel");
            }
        }
    }
}
