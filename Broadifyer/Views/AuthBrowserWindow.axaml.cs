using Avalonia.Controls;
using WebViewControl;

namespace Broadifyer.Views
{
    public partial class AuthBrowserWindow : Window
    {
        public AuthBrowserWindow()
        {
            WebView.Settings.OsrEnabled = false;
            WebView.Settings.LogFile = "BrowserLog.txt";
            InitializeComponent();
        }
    }
}
