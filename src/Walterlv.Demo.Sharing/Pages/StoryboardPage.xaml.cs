#if WINDOWS_UWP
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Walterlv.ComponentModel;

#else
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Walterlv.Demo.Media.Animation;

#endif

namespace Walterlv.Demo.Pages
{
    public partial class StoryboardPage
    {
        public StoryboardPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;
        }

        private int index = 0;

        private void BeginStoryboard_Click(object sender, RoutedEventArgs e)
        {
            BeginConnectedAnimation((UIElement) sender);
        }

        private void BeginStoryboard2_Click(object sender, RoutedEventArgs e)
        {
            BeginConnectedAnimation((UIElement)sender);
        }

        private void PauseStoryboard_Click(object sender, RoutedEventArgs e)
        {
            BeginConnectedAnimation((UIElement)sender);
        }

        private async void BeginConnectedAnimation(UIElement source)
        {
            source.Visibility = Visibility.Hidden;
            ConnectionDestination.Visibility = Visibility.Hidden;
            var animatingSource = (UIElement) VisualTreeHelper.GetChild(source, 0);
            var animatingDestination = (UIElement)VisualTreeHelper.GetChild(ConnectionDestination, 0);

            var service = ConnectedAnimationService.GetForCurrentView(this);
            service.PrepareToAnimate($"Test{index}", animatingSource);
            var animation = service.GetAnimation($"Test{index}");
            animation?.TryStart(animatingDestination);
            index++;

            await Task.Delay(10000);
            source.ClearValue(VisibilityProperty);
            ConnectionDestination.ClearValue(VisibilityProperty);
        }
    }

    public static class GeometryExtensions
    {
        public static double GetProximateLength(this Geometry geometry)
        {
            //var path = geometry.GetFlattenedPathGeometry();
            var length = 0.0;
            //foreach (var figure in path.Figures)
            //{
            //    var start = figure.StartPoint;
            //    foreach (var segment in figure.Segments)
            //    {
            //        if (segment is PolyLineSegment polyLine)
            //        {
            //            // 一般的路径会转换成折线。
            //            foreach (var point in polyLine.Points)
            //            {
            //                length += ProximateDistance(start, point);
            //                start = point;
            //            }
            //        }
            //        else if (segment is LineSegment line)
            //        {
            //            // 少部分真的是线段的路径会转换成线段。
            //            length += ProximateDistance(start, line.Point);
            //            start = line.Point;
            //        }
            //    }
            //}
            return length;

            double ProximateDistance(Point p1, Point p2)
            {
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            }
        }
    }
}
