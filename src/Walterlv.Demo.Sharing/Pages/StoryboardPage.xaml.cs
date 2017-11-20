#if WINDOWS_UWP
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

#else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        private Storyboard TranslateStoryboard => (Storyboard)FindResource("Storyboard.Translate");
        private Storyboard DrawLineStoryboard => (Storyboard) FindResource("Storyboard.DrawName");
#endif

        private DoubleAnimation TranslateXAnimation => (DoubleAnimation) TranslateStoryboard.Children[0];

        private DoubleAnimation TranslateYAnimation => (DoubleAnimation) TranslateStoryboard.Children[1];

        private readonly Random _random = new Random(DateTime.Now.Ticks.GetHashCode());

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;
            TranslateStoryboard.Begin();
            TranslateStoryboard.Stop();

            for (var i = 0; i < DrawLineStoryboard.Children.Count; i++)
            {
                InitializePathAndItsAnimation((Path) DisplayCanvas.Children[i + 2], (DoubleAnimation) DrawLineStoryboard.Children[i]);
            }
            DrawLineStoryboard.Begin();
        }

        private void InitializePathAndItsAnimation(Path path, DoubleAnimation animation)
        {
            var length = path.Data.GetProximateLength() / path.StrokeThickness;
            path.StrokeDashOffset = length;
            path.StrokeDashArray = new DoubleCollection(new[] {length, length});
            animation.From = length;
        }

        private void BeginStoryboard_Click(object sender, RoutedEventArgs e)
        {
            Uwp_AnimateToRandomPosition();
            TranslateStoryboard.Begin();
            MoveToRandomPosition();
        }

        private void BeginStoryboard2_Click(object sender, RoutedEventArgs e)
        {
            MoveToRandomPosition();
            Uwp_AnimateToRandomPosition();
            TranslateStoryboard.Begin();
            MoveToRandomPosition();
        }

        private void PauseStoryboard_Click(object sender, RoutedEventArgs e)
        {
            TranslateStoryboard.Pause();
        }

        [Conditional("WINDOWS_UWP")]
        private void Uwp_AnimateToRandomPosition()
        {
            var nextPosition = NextRandomPosition();
            TranslateXAnimation.To = nextPosition.X;
            TranslateYAnimation.To = nextPosition.Y;
        }

        [Conditional("WPF")]
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

    public static class GeometryExtensions
    {
        public static double GetProximateLength(this Geometry geometry)
        {
            var path = geometry.GetFlattenedPathGeometry();
            var length = 0.0;
            foreach (var figure in path.Figures)
            {
                var start = figure.StartPoint;
                foreach (var segment in figure.Segments)
                {
                    if (segment is PolyLineSegment polyLine)
                    {
                        // 一般的路径会转换成折线。
                        foreach (var point in polyLine.Points)
                        {
                            length += ProximateDistance(start, point);
                            start = point;
                        }
                    }
                    else if (segment is LineSegment line)
                    {
                        // 少部分真的是线段的路径会转换成线段。
                        length += ProximateDistance(start, line.Point);
                        start = line.Point;
                    }
                }
            }
            return length;

            double ProximateDistance(Point p1, Point p2)
            {
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            }
        }
    }
}
