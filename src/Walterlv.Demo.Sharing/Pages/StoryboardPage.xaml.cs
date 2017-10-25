#if WINDOWS_UWP
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#else
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

#endif

namespace Walterlv.Demo.Pages
{
    public partial class StoryboardPage : Page
    {
        public StoryboardPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

#if !WINDOWS_UWP
        private Storyboard TranslateStoryboard => (Storyboard) FindResource("Storyboard.Translate");
#endif

        private readonly Random _random = new Random(DateTime.Now.Ticks.GetHashCode());

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            TranslateStoryboard.Begin();
            TranslateStoryboard.Stop();
        }

        private void BeginStoryboard_Click(object sender, RoutedEventArgs e)
        {
            TranslateStoryboard.Begin();
            MoveToRandomPosition();
        }

        private void BeginStoryboard2_Click(object sender, RoutedEventArgs e)
        {
            MoveToRandomPosition();
            TranslateStoryboard.Begin();
            MoveToRandomPosition();
        }

        private void PauseStoryboard_Click(object sender, RoutedEventArgs e)
        {
            TranslateStoryboard.Pause();
        }

        private void MoveToRandomPosition()
        {
            var nextPosition = NextRandomPosition();
            TranslateTransform.X = nextPosition.X;
            TranslateTransform.Y = nextPosition.Y;
        }

        private Point NextRandomPosition()
        {
            var areaX = (int) Math.Round(DisplayCanvas.ActualWidth - DisplayShape.ActualWidth);
            var areaY = (int) Math.Round(DisplayCanvas.ActualHeight - DisplayShape.ActualHeight);
            return new Point(_random.Next(areaX) + 1, _random.Next(areaY) + 1);
        }
    }
}
