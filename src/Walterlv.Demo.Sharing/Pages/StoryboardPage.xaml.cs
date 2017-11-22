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
                    if (TraceShape.RenderTransformOrigin == default(Point))
                    {
                        return (new Point(), new Point(
                            DisplayShape.ActualWidth * scalingFactor.X / 2,
                            DisplayShape.ActualHeight * scalingFactor.Y / 2));
                    }
                    else
                    {
                        if (index % 2 == 0)
                        {
                            return (new Point(),
                                new Point(
                                    DisplayShape.ActualWidth * scalingFactor.X / 2,
                                    DisplayShape.ActualHeight * scalingFactor.Y / 2));
                        }
                        else
                        {
                            return (new Point(),
                                new Point(
                                    DisplayShape.ActualWidth * (scalingFactor.X - 0) / 2,
                                    DisplayShape.ActualHeight * (scalingFactor.Y - 0) / 2));
                        }
                    }
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
                if (TraceShape.RenderTransformOrigin == default(Point))
                {
                    var scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform != null)
                    {
                        scaleTransform.CenterX = 0;
                        scaleTransform.CenterY = 0;
                    }
                    var rotateTransform = group.Children.OfType<RotateTransform>().FirstOrDefault();
                    if (rotateTransform != null)
                    {
                        rotateTransform.CenterX = DisplayShape.ActualWidth * scaling.X / 2;
                        rotateTransform.CenterY = DisplayShape.ActualHeight * scaling.Y / 2;
                    }
                }
                else
                {
                    var scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform != null)
                    {
                        scaleTransform.CenterX = -DisplayShape.ActualWidth / 2;
                        scaleTransform.CenterY = -DisplayShape.ActualHeight / 2;
                    }
                    var rotateTransform = group.Children.OfType<RotateTransform>().FirstOrDefault();
                    if (rotateTransform != null && index % 2 != 0)
                    {
                        rotateTransform.CenterX = DisplayShape.ActualWidth * (scaling.X - 1) / 2;
                        rotateTransform.CenterY = DisplayShape.ActualHeight * (scaling.Y - 1) / 2;
                    }
                }
                group.Children.Add(new TranslateTransform {X = translation.X, Y = translation.Y});
                TraceShape.RenderTransform = group;
                index++;
            }
        }

        /// <summary>
        /// 对于给定的变换矩阵 <paramref name="matrix"/>，求出一个可以模拟此变换矩阵的缩放、旋转、平移变换组。
        /// <para>默认情况下变换中心都是元素的左上角点 (0, 0)，也可以通过指定参数 <paramref name="specifyCenter"/> 修改这一行为。</para>
        /// </summary>
        /// <param name="matrix">需要解构的变换矩阵（如果是对 UI 元素进行解构，传入的 Matrix 需要额外考虑 RenderTransformOrigin。）</param>
        /// <param name="specifyCenter">
        /// 如果需要分别为缩放和旋转指定变换中心，则在此参数中通过委托给定。
        /// <para>委托需要返回两个值，第一个是缩放中心（绝对坐标），相对于未进行任何变换时的元素；第二个是旋转中心（绝对坐标），相对于缩放后的元素（缩放比可通过委托参数获取）。</para>
        /// </param>
        /// <returns>按缩放、旋转、平移顺序返回变换参数，可直接应用到对应的 <see cref="Transform"/> 中。</returns>
        private (Vector Scaling, double Rotation, Vector Translation) ExtractMatrix(
            Matrix matrix, CenterSpecification specifyCenter = null)
        {
            // 生成一个单位矩形（0, 0, 1, 1），计算单位矩形经矩阵变换后形成的带旋转的矩形。
            // 于是，我们将可以通过比较这两个矩形中点的数据来求出一个解。
            var unitPoints = new[] {new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)};
            var transformedPoints = unitPoints.Select(matrix.Transform).ToArray();

            // 测试单位矩形宽高的长度变化量，以求出缩放比（作为参数 specifyCenter 中变换中心的计算参考）。
            var scaling = new Vector(
                (transformedPoints[1] - transformedPoints[0]).Length,
                (transformedPoints[3] - transformedPoints[0]).Length);
            // 测试单位向量的旋转变化量，以求出旋转角度。
            var rotation = Vector.AngleBetween(new Vector(1, 0), transformedPoints[1] - transformedPoints[0]);
            var translation = transformedPoints[0] - unitPoints[0];

            // 如果指定了变换分量的变换中心点。
            if (specifyCenter != null)
            {
                // 那么，就获取指定的变换中心点（缩放中心和旋转中心）。
                var (scalingCenter, rotationCenter) = specifyCenter(scaling);

                // 如果 S 表示所求变换的缩放分量，R 表示所求变换的旋转分量，T 表示所求变换的平移分量；M 表示传入的目标矩阵。
                // 那么，S 将可以通过缩放比和参数指定的缩放中心唯一确定；R 将可以通过旋转角度和参数指定的旋转中心唯一确定。
                // S = scaleMatrix; R = rotateMatrix.
                var scaleMatrix = Matrix.Identity;
                scaleMatrix.ScaleAt(scaling.X, scaling.Y, scalingCenter.X, scalingCenter.Y);
                var rotateMatrix = Matrix.Identity;
                rotateMatrix.RotateAt(rotation, rotationCenter.X, rotationCenter.Y);

                // T 是不确定的，它会受到 S 和 T 的影响；但确定等式 SRT=M，即 T=S^{-1}R^{-1}M。
                // T = translateMatrix; M = matrix.
                scaleMatrix.Invert();
                rotateMatrix.Invert();
                var translateMatrix = Matrix.Multiply(rotateMatrix, scaleMatrix);
                translateMatrix = Matrix.Multiply(translateMatrix, matrix);

                // 用考虑了变换中心的平移量覆盖总的平移分量。
                translation = new Vector(translateMatrix.OffsetX, translateMatrix.OffsetY);
            }

            // 按缩放、旋转、平移来返回变换分量。
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
