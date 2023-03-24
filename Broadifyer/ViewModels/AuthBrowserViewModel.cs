using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broadifyer.ViewModels
{
    public class AuthBrowserViewModel : ViewModelBase
    {
        public Uri Address { get; protected set; }

        public AuthBrowserViewModel(Uri auth_uri)
        {
            Address = auth_uri;
        }
    }
}
