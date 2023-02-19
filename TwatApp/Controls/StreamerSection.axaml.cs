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
        public static readonly StyledProperty<ICommand> ToggleEnableCommandProperty =
            AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(ToggleEnableCommand));

        public static readonly StyledProperty<ICommand> RemoveCommandProperty =
            AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(RemoveCommand));

        public static readonly StyledProperty<ICommand> AddCommandProperty =
            AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(AddCommand));

        public static readonly StyledProperty<ICommand> AddFollowedCommandProperty =
            AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(AddFollowedCommand));

        public static readonly StyledProperty<ICommand> StreamerModifiedProperty =
            AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(StreamerModified));

        public static readonly StyledProperty<string> StreamerInputProperty =
            AvaloniaProperty.Register<StreamerSection, string>(nameof(StreamerInput), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<IList<StreamerViewModel>> StreamersProperty =
            AvaloniaProperty.Register<StreamerSection, IList<StreamerViewModel>>(nameof(Streamers));

        public static readonly StyledProperty<StreamerViewModel?> SelectedStreamerProperty =
            AvaloniaProperty.Register<StreamerSection, StreamerViewModel?>(nameof(SelectedStreamer), defaultBindingMode: BindingMode.OneWayToSource);

        public StreamerSection()
        {
            InitializeComponent();
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

		public IList<StreamerViewModel> Streamers
		{
			get => GetValue(StreamersProperty);
			set => SetValue(StreamersProperty, value);
		}

        public StreamerViewModel? SelectedStreamer
        {
            get => GetValue(SelectedStreamerProperty);
            set => SetValue(SelectedStreamerProperty, value);
        }
    }
}
