using Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using TwatApp.Models;

namespace TwatApp.Controls
{
    public partial class CategorySection : UserControl
    {
        public CategorySection()
        {
            InitializeComponent();

            StreamerNameProperty.Changed.AddClassHandler<CategorySection>((x, y) => Trace.WriteLine("TTTT"));

		}

		public static readonly StyledProperty<IStreamer?> StreamerProperty =
			AvaloniaProperty.Register<CategorySection, IStreamer?>(nameof(Streamer));


		public IStreamer? Streamer
		{
			get => GetValue(StreamerProperty);
			set => SetValue(StreamerProperty, value);
		}


		public static readonly StyledProperty<string> StreamerNameProperty =
			AvaloniaProperty.Register<CategorySection, string>(nameof(StreamerName));

		public string StreamerName
		{
			get => GetValue(StreamerNameProperty);
			set => SetValue(StreamerNameProperty, value);
		}


		public static readonly StyledProperty<string> CategoryInputProperty =
			AvaloniaProperty.Register<CategorySection, string>(nameof(CategoryInput));

		public string CategoryInput
		{
			get => GetValue(CategoryInputProperty);
			set => SetValue(CategoryInputProperty, value);
		}



	}
}
