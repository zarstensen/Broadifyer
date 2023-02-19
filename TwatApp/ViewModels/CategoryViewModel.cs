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
    public class CategoryViewModel : ReactiveObject
    {
        public Expose<bool, ICategoryInfo> Enable { get; set; }
        //public Bitmap? Icon { get => m_category_info.Icon; }
        public string Name { get => m_category_info.Category.Name; }

        public CategoryViewModel(ICategoryInfo category_info)
        {
            m_category_info = category_info;
            Enable = new(m_category_info, nameof(m_category_info.Enable));
        }

        protected ICategoryInfo m_category_info;
    }

    //public class WhitelistedCategories : ReactiveObject
    //{
    //    AvaloniaList<CategoryViewModel> WhitelistedCategories { get; }
    //}
}
