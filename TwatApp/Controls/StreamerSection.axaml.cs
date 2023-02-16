using Avalonia;
using Avalonia.Controls;
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
			AvaloniaProperty.Register<StreamerSection, string>(nameof(StreamerInput));

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



	}
}
