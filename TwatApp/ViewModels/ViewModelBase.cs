using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwatApp.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public AppViewModel AppVM { get => (App.Current!.DataContext as AppViewModel)!; }
    }
}
