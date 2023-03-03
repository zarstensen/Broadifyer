using Avalonia;
using Avalonia.Controls;
using TwatApp.ViewModels;

namespace TwatApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                Hide();
                e.Cancel = true;
            };
        }

    }
}
