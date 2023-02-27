using Avalonia.Collections;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;

namespace TwatApp.ViewModels
{
    public class CategoryViewModel : ReactiveObject, IViewModel<ICategoryInfo>
    {
        public Expose<bool, ICategoryInfo> Enable { get; set; }
        //public Bitmap? Icon { get => m_category_info.Icon; }
        public string Name { get => category_info.Category.Name; }

        public CategoryViewModel() { }

        public CategoryViewModel(ICategoryInfo category_info)
        {
            supplyModel(category_info);
        }

        public ICategoryInfo category_info;

        public void supplyModel(ICategoryInfo model)
        {
            this.category_info = model;
            Enable = new(category_info, nameof(category_info.Enable));
        }
    }
}
