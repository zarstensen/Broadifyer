using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using DynamicData.Kernel;
using DynamicData.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using TwatApp.Models;
using TwatApp.ViewModels;

namespace TwatApp.Controls
{
    public partial class StreamerSection : UserControl
    {
		public StreamerSection()
        {
            InitializeComponent();
        }


		public static readonly StyledProperty<ICommand> ToggleEnableCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(ToggleEnableCommand));

		public ICommand ToggleEnableCommand
		{
			get => GetValue(ToggleEnableCommandProperty);
			set => SetValue(ToggleEnableCommandProperty, value);
		}


		public static readonly StyledProperty<ICommand> RemoveCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(RemoveCommand));

		public ICommand RemoveCommand
		{
			get => GetValue(RemoveCommandProperty);
			set => SetValue(RemoveCommandProperty, value);
		}


		public static readonly StyledProperty<ICommand> AddCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(AddCommand));

		public ICommand AddCommand
		{
			get => GetValue(AddCommandProperty);
			set => SetValue(AddCommandProperty, value);
		}


		public static readonly StyledProperty<ICommand> AddFollowedCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(AddFollowedCommand));

		public ICommand AddFollowedCommand
		{
			get => GetValue(AddFollowedCommandProperty);
			set => SetValue(AddFollowedCommandProperty, value);
		}


		public static readonly StyledProperty<ICommand> StreamerModifiedProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(StreamerModified));

		public ICommand StreamerModified
		{
			get => GetValue(StreamerModifiedProperty);
			set => SetValue(StreamerModifiedProperty, value);
		}


		public static readonly StyledProperty<string> StreamerInputProperty =
			AvaloniaProperty.Register<StreamerSection, string>(nameof(StreamerInput), defaultBindingMode: BindingMode.TwoWay);

		public string StreamerInput
		{
			get => GetValue(StreamerInputProperty);
			set => SetValue(StreamerInputProperty, value);
		}

		public static readonly StyledProperty<IList<StreamerViewModel>> StreamersProperty =
			AvaloniaProperty.Register<StreamerSection, IList<StreamerViewModel>>(nameof(Streamers));

		public IList<StreamerViewModel> Streamers
		{
			get => GetValue(StreamersProperty);
			set => SetValue(StreamersProperty, value);
		}

        public static readonly StyledProperty<StreamerViewModel?> SelectedStreamerProperty =
            AvaloniaProperty.Register<StreamerSection, StreamerViewModel?>(nameof(SelectedStreamer), defaultBindingMode: BindingMode.OneWayToSource);

        public StreamerViewModel? SelectedStreamer
        {
            get => GetValue(SelectedStreamerProperty);
            set => SetValue(SelectedStreamerProperty, value);
        }
    }
}
