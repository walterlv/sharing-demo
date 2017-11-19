using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Walterlv.ComponentModel;

namespace Walterlv.Demo.Pages
{
    public partial class WindowMessagePage : Page
    {
        public WindowMessagePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }

    public sealed class WindowMessageViewModel : NotificationObject
    {
        private int _value;

        public int Value
        {
            get => _value;
            set => SetValue(ref _value, value);
        }

        
    }
}
