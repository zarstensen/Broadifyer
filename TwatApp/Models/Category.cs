using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace TwatApp.Models
{
    /// <summary>
    /// represents identification values for a specific twitch category.
    /// </summary>
    public interface ICategory : IEqualityComparer<ICategory>
    {
        /// <summary>
        /// id of the category
        /// </summary>
        string Id { get; }
        /// <summary>
        /// name of the category
        /// </summary>
        string Name { get; }

        string IconUri { get; }
        /// <summary>
        /// path to where the category icon file will be stored.
        /// </summary>
        [JsonIgnore]
        public string IconFile { get; }

        bool IEqualityComparer<ICategory>.Equals(ICategory? x, ICategory? y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;
        }

        int IEqualityComparer<ICategory>.GetHashCode(ICategory obj)
        {
            return obj.Id.GetHashCode();
        }

    }

    /// <summary>
    /// stores various (a single field) data.
    /// there can be multiple ICategoryInfo instances for a single ICategory instance,
    /// as these hold streamer dependent information and not category dependent information.
    /// </summary>
    public interface ICategoryInfo
    {
        public ICategory Category { get; }
        public bool Enable { get; set; }
        [JsonIgnore]
        public Bitmap? Icon { get; }
        public Task prepareIcons();
    }
}
