using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BroadifyerApp.Models;

namespace BroadifyerApp.ViewModels
{
    public class CategoryVM : ReactiveObject
    {
        public Expose<bool, ICategoryInfo> Enable { get; set; }
        public Bitmap? Icon { get => category_info.Icon; }
        public string Name { get => category_info.Category.Name; }

        public CategoryVM(ICategoryInfo model)
        {
            category_info = model;
            Enable = new(category_info, nameof(category_info.Enable));
        }

        public ICategoryInfo category_info;
    }

    public class CategoriesViewModel : ViewModelBase
    {
        public React<StreamerVM?> TargetStreamer { get; set; }
        public string CategoryInput { get; set; } = "";
        
        public CategoriesViewModel(TwitchNotify notifier, React<StreamerVM?> target_streamer)
        {
            TargetStreamer = target_streamer;
            m_notifier = notifier;
            TargetStreamer.Value?.sortCategories();
        }

        /// <summary>
        /// attempt to add a category with the name stored in CategoryInput, and associate it with the passed streamer.
        /// </summary>
        /// <param name="streamer"></param>
        public async void addCategory()
        {
            if (CategoryInput == "")
            {
                await WindowVM.showInfo($"Category name cannot be empty.", 5000);
                return;
            }

            var found_category = await m_notifier.categoryFromName(CategoryInput);

            if (found_category == null || TargetStreamer.Value == null)
            {
                await WindowVM.showInfo($"Could not find a category named {CategoryInput}!", 5000);
                return;
            }

            var cinfo = m_notifier.filterCategory(found_category, TargetStreamer.Value.streamer_info.Streamer);
            await cinfo.prepareIcons();

            TargetStreamer.Value.FilteredCategories.Add(new(cinfo));
            TargetStreamer.Value.sortCategories();
            m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
        }

        public void removeCategory(CategoryVM category)
        {
            TargetStreamer.Value?.FilteredCategories.Remove(category);
            TargetStreamer.Value?.streamer_info.FilteredCategories.Remove(category.category_info.Category.Id);
            TargetStreamer.Value?.sortCategories();
            m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
        }

        public void WhiteBlacklistChanged() => m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);

        protected TwitchNotify m_notifier;
    }
}
