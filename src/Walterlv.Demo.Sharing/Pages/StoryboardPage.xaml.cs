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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Converters;
using Walterlv.ComponentModel;

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

#if !WINDOWS_UWP
        private Storyboard TranslateStoryboard => (Storyboard)FindResource("Storyboard.Translate");
        private Storyboard DrawLineStoryboard => (Storyboard) FindResource("Storyboard.DrawName");
#endif

        private DoubleAnimation TranslateXAnimation => (DoubleAnimation) TranslateStoryboard.Children[0];

        private DoubleAnimation TranslateYAnimation => (DoubleAnimation) TranslateStoryboard.Children[1];

        private readonly Random _random = new Random(DateTime.Now.Ticks.GetHashCode());

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;
            TranslateStoryboard.Begin();
            TranslateStoryboard.Stop();

            for (var i = 0; i < DrawLineStoryboard.Children.Count; i++)
            {
                InitializePathAndItsAnimation((Path) DisplayCanvas.Children[i + 2],
                    (DoubleAnimation) DrawLineStoryboard.Children[i]);
            }
            DrawLineStoryboard.Begin();

            var index = 0;
            while (true)
            {
                await Task.Delay(500);

                var matrix = DisplayShape.RenderTransform.Value;
                var (scaling, rotation, translation) = ExtractMatrix(matrix, (scalingFactor) =>
                {
                    return (new Point(0, 0),
                        new Point());
                });
                var group = new TransformGroup();
                if (index % 2 == 0)
                {
                    TraceShape.Width = DisplayShape.ActualWidth * scaling.X;
                    TraceShape.Height = DisplayShape.ActualHeight * scaling.Y;
                }
                else
                {
                    TraceShape.Width = DisplayShape.ActualWidth;
                    TraceShape.Height = DisplayShape.ActualHeight;
                    group.Children.Add(new ScaleTransform
                    {
                        ScaleX = scaling.X,
                        ScaleY = scaling.Y,
                    });
                }
                group.Children.Add(new RotateTransform
                {
                    Angle = rotation,
                });
                if (TraceShape.RenderTransformOrigin == new Point(0.5, 0.5))
                {
                    var scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform != null)
                    {
                        scaleTransform.CenterX = -DisplayShape.ActualWidth / 2;
                        scaleTransform.CenterY = -DisplayShape.ActualHeight / 2;
                    }
                    var rotateTransform = group.Children.OfType<RotateTransform>().FirstOrDefault();
                    if (rotateTransform != null)
                    {
                        rotateTransform.CenterX = DisplayShape.ActualWidth * scaling.X / 2;
                        rotateTransform.CenterY = DisplayShape.ActualHeight * scaling.Y / 2;
                    }
                }
                group.Children.Add(new TranslateTransform {X = translation.X, Y = translation.Y});
                TraceShape.RenderTransform = group;
                index++;
            }
        }

        private (Vector Scaling, double Rotation, Vector Translation) ExtractMatrix(
            Matrix matrix, CenterSpecification specifyCenter = null)
        {
            var unitPoints = new[] {new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)};
            var transformedPoints = unitPoints.Select(matrix.Transform).ToArray();
            var scaling = new Vector(
                (transformedPoints[1] - transformedPoints[0]).Length,
                (transformedPoints[3] - transformedPoints[0]).Length);
            var rotation = Vector.AngleBetween(new Vector(1, 0), transformedPoints[1] - transformedPoints[0]);
            var translation = transformedPoints[0] - unitPoints[0];
            if (specifyCenter != null)
            {
                var (scalingCenter, rotationCenter) = specifyCenter(scaling);

                var scaleMatrix = Matrix.Identity;
                scaleMatrix.ScaleAt(scaling.X, scaling.Y, scalingCenter.X, scalingCenter.Y);
                var rotateMatrix = Matrix.Identity;
                rotateMatrix.RotateAt(rotation, rotationCenter.X, rotationCenter.Y);

                scaleMatrix.Invert();
                rotateMatrix.Invert();
                var translateMatrix = Matrix.Multiply(rotateMatrix, scaleMatrix);
                translateMatrix = Matrix.Multiply(translateMatrix, matrix);
                translation = new Vector(translateMatrix.OffsetX, translateMatrix.OffsetY);

                //var fixMatrix = new Matrix(1, 0, 0, 1, rotationCenter.X, rotationCenter.Y);
                //var invertedFixMatrix = fixMatrix;
                //invertedFixMatrix.Invert();
                //var fix = Matrix.Multiply(invertedFixMatrix, matrix);
                //fix = Matrix.Multiply(fix, fixMatrix);
                //translation = new Vector(fix.OffsetX, fix.OffsetY);

                //// 如果目标自带缩放中心，那么此处需要先去掉缩放中心带来的平移分量。
                //translation += Scale(scalingCenter, scaling) - scalingCenter;
                //// 如果目标自带旋转中心，那么此处需要先去掉旋转中心带来的平移分量。
                //translation += Rotate(rotationCenter, rotation) - rotationCenter;
            }
            return (scaling, rotation, translation);
        }

        public delegate (Point ScalingCenter, Point RotationCenter) CenterSpecification(Vector scalingFactor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point Scale(Point point, Vector scale)
        {
            return new Point(point.X * scale.X, point.Y * scale.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point Rotate(Point point, double angle)
        {
            var matrix = Matrix.Identity;
            matrix.Rotate(angle);
            return matrix.Transform(point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector Rotate(Vector vector, double angle)
        {
            var matrix = Matrix.Identity;
            matrix.Rotate(angle);
            return matrix.Transform(vector);
        }

        private void InitializePathAndItsAnimation(Path path, DoubleAnimation animation)
        {
            //var length = path.Data.GetProximateLength() / path.StrokeThickness;
            //path.StrokeDashOffset = length;
            //path.StrokeDashArray = new DoubleCollection(new[] {length, length});
            //animation.From = length;
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
