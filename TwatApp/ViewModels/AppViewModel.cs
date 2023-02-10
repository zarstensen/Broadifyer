using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.ViewModels
{
    

    public class AppViewModel : ViewModelBase
    {
        public AppViewModel()
        {
        }

        public void ExitCommand()
        {
            IClassicDesktopStyleApplicationLifetime lifetime = (IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime;
            lifetime.TryShutdown();
        }
    }
}
