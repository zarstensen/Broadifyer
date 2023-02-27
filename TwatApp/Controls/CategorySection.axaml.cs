using Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using System.Windows.Input;
using TwatApp.Models;
using TwatApp.ViewModels;

namespace TwatApp.Controls
{
    public partial class CategorySection : UserControl
    {
        public static readonly StyledProperty<ICommand> AddCommandProperty =
    AvaloniaProperty.Register<CategorySection, ICommand>(nameof(AddCommand));

        public static readonly StyledProperty<StreamerViewModel?> StreamerProperty =
    AvaloniaProperty.Register<CategorySection, StreamerViewModel?>(nameof(Streamer));

        public static readonly StyledProperty<string> CategoryInputProperty =
    AvaloniaProperty.Register<CategorySection, string>(nameof(CategoryInput));

		public CategorySection()
        {
            InitializeComponent();
		}

		public ICommand AddCommand
		{
			get => GetValue(AddCommandProperty);
			set => SetValue(AddCommandProperty, value);
		}

		public StreamerViewModel? Streamer
		{
			get => GetValue(StreamerProperty);
			set
			{
				SetValue(StreamerProperty, value);
				Trace.WriteLine("UPDATE CATEGORIES");
			}
		}

		public string CategoryInput
		{
			get => GetValue(CategoryInputProperty);
			set => SetValue(CategoryInputProperty, value);
		}
	}
}
