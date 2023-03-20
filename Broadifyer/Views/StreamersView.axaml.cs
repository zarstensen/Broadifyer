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
using Broadifyer.Models;
using Broadifyer.ViewModels;

namespace Broadifyer.Views
{
    public partial class StreamersView : UserControl
    {
        public StreamersView()
        {
            InitializeComponent();
        }
    }
}
