using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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
            

            await TestLoadUnloadIssueAsync();
        }

        private async Task TestLoadUnloadIssueAsync()
        {
            var elementA = new TextBlock
            {
                Text = "A",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var elementB = new TextBlock
            {
                Text = "B",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var elementC = new Rectangle
            {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(Colors.Tomato) {Opacity = 0.5},
            };
            var groupD = new Grid();
            groupD.Children.Add(elementB);
            groupD.Children.Add(elementC);
            LoadUnloadTestingPanel.Children.Add(elementA);
            LoadUnloadTestingPanel.Children.Add(groupD);

            for (var i = 0; i < 100; i++)
            {
                await Task.Delay(1000);
                await Ungroup();
                await Dispatcher.Yield(DispatcherPriority.Input);
                await Group();
            }

            async Task Group()
            {
                var groupE = (Grid) LoadUnloadTestingPanel.Children[0];
                LoadUnloadTestingPanel.Children.Remove(groupE);
                await Dispatcher.Yield();
                groupE.Children.Remove(elementA);
                await Dispatcher.Yield();
                LoadUnloadTestingPanel.Children.Add(elementA);
                await Dispatcher.Yield();
                groupE.Children.Remove(groupD);
                await Dispatcher.Yield();
                LoadUnloadTestingPanel.Children.Add(groupD);
            }

            async Task Ungroup()
            {
                var groupE = new Grid();
                LoadUnloadTestingPanel.Children.Remove(elementA);
                await Dispatcher.Yield();
                groupE.Children.Add(elementA);
                await Dispatcher.Yield();
                LoadUnloadTestingPanel.Children.Remove(groupD);
                await Dispatcher.Yield();
                groupE.Children.Add(groupD);
                await Dispatcher.Yield();
                LoadUnloadTestingPanel.Children.Add(groupE);
            }
        }
    }
}
