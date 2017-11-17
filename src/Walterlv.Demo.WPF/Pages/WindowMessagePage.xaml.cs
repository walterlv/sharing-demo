using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

    public sealed class WindowMessageViewModel
    {
        
    }
}
