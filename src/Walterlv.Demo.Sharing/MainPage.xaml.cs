#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Walterlv.Demo.Pages;
#else
using System;
using System.Windows;
using System.Windows.Controls;
using Walterlv.Demo.Pages;
#endif

namespace Walterlv.Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
#if WINDOWS_UWP
            MainFrame.Navigate(typeof(StoryboardPage));
#else
            MainFrame.Navigate(new StoryboardPage());
#endif
        }
    }
}
