using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BroadifyerApp.Models;

namespace BroadifyerApp.ViewModels
{
    public class ConfigEditorViewModel : ViewModelBase
    {
        public ViewModelBase StreamersView {
            get => streamers_view.Value;
        }

        public CategoriesViewModel CategoriesView
        {
            get => categories_view.Value;
        }
    

        public React<StreamersViewModel> streamers_view = new();
        public React<CategoriesViewModel> categories_view = new();

        public ConfigEditorViewModel(TwitchNotify notifier)
        {
            streamers_view.Value = new StreamersViewModel(notifier);
            categories_view.Value = new CategoriesViewModel(notifier, streamers_view.Value.SelectedStreamer);
        }
        
        public void openGithub() => Process.Start(new ProcessStartInfo() {
            FileName = "https://github.com/karstensensensen/Broadifyer",
            UseShellExecute = true });
    }
}
