using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwatApp.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
    }

    public interface IViewModel<TModel>
    {
        public void supplyModel(TModel model);
    }
}
