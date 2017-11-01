using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Walterlv.Demo.Pages
{
    public partial class MultiThreadPage : Page
    {
        public MultiThreadPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            await TargetHost.SetChildAsync(() =>
            {
                var box = new TextBox
                {
                    Text = "吕毅 - walterlv",
                    Width = 300,
                    Height = 120,
                    Background = new SolidColorBrush(Colors.White) {Opacity = 0.5},
                };
                return box;
            });
        }
    }
}
