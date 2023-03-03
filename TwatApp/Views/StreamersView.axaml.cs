using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using DynamicData.Kernel;
using DynamicData.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using TwatApp.Models;
using TwatApp.ViewModels;

namespace TwatApp.Views
{
    public partial class StreamersView : UserControl
    {
        public static readonly StyledProperty<ICommand> ToggleEnableCommandProperty =
            AvaloniaProperty.Register<StreamersView, ICommand>(nameof(ToggleEnableCommand));

        public static readonly StyledProperty<ICommand> RemoveCommandProperty =
            AvaloniaProperty.Register<StreamersView, ICommand>(nameof(RemoveCommand));

        public static readonly StyledProperty<ICommand> AddCommandProperty =
            AvaloniaProperty.Register<StreamersView, ICommand>(nameof(AddCommand));

        public static readonly StyledProperty<ICommand> AddFollowedCommandProperty =
            AvaloniaProperty.Register<StreamersView, ICommand>(nameof(AddFollowedCommand));

        public static readonly StyledProperty<ICommand> StreamerModifiedProperty =
            AvaloniaProperty.Register<StreamersView, ICommand>(nameof(StreamerModified));

        public static readonly StyledProperty<string> StreamerInputProperty =
            AvaloniaProperty.Register<StreamersView, string>(nameof(StreamerInput), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<IList<StreamerVM>> StreamersProperty =
            AvaloniaProperty.Register<StreamersView, IList<StreamerVM>>(nameof(Streamers));

        public static readonly StyledProperty<StreamerVM?> SelectedStreamerProperty =
            AvaloniaProperty.Register<StreamersView, StreamerVM?>(nameof(SelectedStreamer), defaultBindingMode: BindingMode.OneWayToSource);

        public StreamersView()
        {
            InitializeComponent();
			SelectedStreamerProperty.Changed.Subscribe(x => Trace.WriteLine($"val: {x.NewValue}"));
        }



        public ICommand ToggleEnableCommand
		{
			get => GetValue(ToggleEnableCommandProperty);
			set => SetValue(ToggleEnableCommandProperty, value);
		}


		public ICommand RemoveCommand
		{
			get => GetValue(RemoveCommandProperty);
			set => SetValue(RemoveCommandProperty, value);
		}

		public ICommand AddCommand
		{
			get => GetValue(AddCommandProperty);
			set => SetValue(AddCommandProperty, value);
		}


		public ICommand AddFollowedCommand
		{
			get => GetValue(AddFollowedCommandProperty);
			set => SetValue(AddFollowedCommandProperty, value);
		}

		public ICommand StreamerModified
		{
			get => GetValue(StreamerModifiedProperty);
			set => SetValue(StreamerModifiedProperty, value);
		}

		public string StreamerInput
		{
			get => GetValue(StreamerInputProperty);
			set => SetValue(StreamerInputProperty, value);
		}

		public IList<StreamerVM> Streamers
		{
			get => GetValue(StreamersProperty);
			set => SetValue(StreamersProperty, value);
		}

        public StreamerVM? SelectedStreamer
        {
            get => GetValue(SelectedStreamerProperty);
            set => SetValue(SelectedStreamerProperty, value);
        }
    }
}
