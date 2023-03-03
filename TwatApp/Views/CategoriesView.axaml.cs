using Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using System.Windows.Input;
using TwatApp.Models;
using TwatApp.ViewModels;

namespace TwatApp.Views
{
    public partial class CategorySection : UserControl
    {


		public static readonly StyledProperty<bool> WhitelistModeProperty =
			AvaloniaProperty.Register<CategorySection, bool>(nameof(WhitelistMode));

		public static readonly StyledProperty<ICommand> RemoveCommandProperty =
			AvaloniaProperty.Register<CategorySection, ICommand>(nameof(RemoveCommand));


		public static readonly StyledProperty<ICommand> AddCommandProperty =
    AvaloniaProperty.Register<CategorySection, ICommand>(nameof(AddCommand));

        public static readonly StyledProperty<StreamerVM?> StreamerProperty =
    AvaloniaProperty.Register<CategorySection, StreamerVM?>(nameof(Streamer));

        public static readonly StyledProperty<string> CategoryInputProperty =
    AvaloniaProperty.Register<CategorySection, string>(nameof(CategoryInput));

		public CategorySection()
        {
            InitializeComponent();
		}

        public bool WhitelistMode
        {
            get => GetValue(WhitelistModeProperty);
            set => SetValue(WhitelistModeProperty, value);
        }


        public ICommand AddCommand
		{
			get => GetValue(AddCommandProperty);
			set => SetValue(AddCommandProperty, value);
		}
        public ICommand RemoveCommand
        {
            get => GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public StreamerVM? Streamer
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
