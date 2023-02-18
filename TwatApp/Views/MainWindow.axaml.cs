using Avalonia;
using Avalonia.Controls;
using TwatApp.Controls;
using TwatApp.ViewModels;

namespace TwatApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var streamer_section = this.FindControl<StreamerSection>("StreamerSection");
            var streamer_list = streamer_section.FindControl<ListBox>("StreamerList");

            var dc = DataContext;

            streamer_list.SelectionChanged += ((MainWindowViewModel)DataContext!).SelectedStreamerChanged;

            //FindControl<StreamerSection>("StreamerSection")
        }
    }
}
