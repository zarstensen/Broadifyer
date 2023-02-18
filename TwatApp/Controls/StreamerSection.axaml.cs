using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using DynamicData.Kernel;
using DynamicData.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using TwatApp.Models;

namespace TwatApp.Controls
{
    public partial class StreamerSection : UserControl
    {
		public StreamerSection()
        {
            InitializeComponent();
			StreamerInputProperty.Changed.AddClassHandler<StreamerSection>((x, y) => Trace.WriteLine(y.NewValue));
        }

		public static readonly StyledProperty<IStreamerInfo?> SelectedStreamerProperty =
			AvaloniaProperty.Register<StreamerSection, IStreamerInfo?>(nameof(SelectedStreamer), defaultBindingMode: BindingMode.OneWayToSource);

		public IStreamerInfo? SelectedStreamer
		{
			get => GetValue(SelectedStreamerProperty);
			set => SetValue(SelectedStreamerProperty, value);
		}

		public static readonly StyledProperty<ICommand> ToggleDisableCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(ToggleDisableCommand));

		public ICommand ToggleDisableCommand
		{
			get => GetValue(ToggleDisableCommandProperty);
			set => SetValue(ToggleDisableCommandProperty, value);
		}


		public static readonly StyledProperty<ICommand> RemoveStreamerCommandProperty =
			AvaloniaProperty.Register<StreamerSection, ICommand>(nameof(RemoveStreamerCommand));

		public ICommand RemoveStreamerCommand
		{
			get => GetValue(RemoveStreamerCommandProperty);
			set => SetValue(RemoveStreamerCommandProperty, value);
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


		public static readonly StyledProperty<string> StreamerInputProperty =
			AvaloniaProperty.Register<StreamerSection, string>(nameof(StreamerInput), defaultBindingMode: BindingMode.TwoWay);

		public string StreamerInput
		{
			get => GetValue(StreamerInputProperty);
			set => SetValue(StreamerInputProperty, value);
		}

		public static readonly StyledProperty<List<IStreamerInfo>> StreamersProperty =
			AvaloniaProperty.Register<StreamerSection, List<IStreamerInfo>>(nameof(Streamers));

		public List<IStreamerInfo> Streamers
		{
			get => GetValue(StreamersProperty);
			set => SetValue(StreamersProperty, value);
		}

		public void StreamerChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs args)
		{
			
			RaisePropertyChanged(SelectedStreamerProperty, Avalonia.Data.Optional<IStreamerInfo?>.Empty, Avalonia.Data.Optional<IStreamerInfo?>.Empty);
		}



	}
}
