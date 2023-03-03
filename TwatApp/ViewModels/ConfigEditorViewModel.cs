using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;

namespace TwatApp.ViewModels
{
    public class ConfigEditorViewModel : ViewModelBase
    {
        public ViewModelBase StreamersV {
            get
            {
                Trace.WriteLine("GOT STUFF");
                
                return streamers_v;
            }
            set => this.RaiseAndSetIfChanged(ref streamers_v, value); }
    

        public ViewModelBase streamers_v;
        //public ViewModelBase CategoriesView { get; set; }
    
        public ConfigEditorViewModel(TwitchNotify notifier)
        {
            StreamersV = new StreamersViewModel(notifier);
        }
    
    }
}
