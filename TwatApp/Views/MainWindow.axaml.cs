using Avalonia;
using Avalonia.Controls;
using TwatApp.ViewModels;

namespace TwatApp.Views
{
    public partial class MainWindow : Window
    {
        public bool start_minimized = false;
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                Hide();
                e.Cancel = true;
            };

            Opened += (s, e) => {
                if (start_minimized)
                {
                    Hide();
                    start_minimized = false;
                }
                };
        }
    }
}
