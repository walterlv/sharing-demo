using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Walterlv.Annotations;

namespace Walterlv.Demo.Media.Animation
{
    public class ConnectedAnimation
    {
        internal ConnectedAnimation(UIElement source)
        {
            _source = source;
        }

        private readonly UIElement _source;

        public bool TryStart([NotNull] UIElement destination)
        {
            return TryStart(destination, Enumerable.Empty<UIElement>());
        }

        public bool TryStart([NotNull] UIElement destination, [NotNull] IEnumerable<UIElement> coordinatedElements)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (coordinatedElements == null)
            {
                throw new ArgumentNullException(nameof(coordinatedElements));
            }
            if (Equals(_source, destination))
            {
                return false;
            }
            // 正在播动画？动画播完废弃了？false

            // 准备播放连接动画。
            var adorner = ConnectedAnimationAdorner.FindFrom(destination);
            var connectionHost = new ConnectedVisual(_source, destination);
            adorner.Children.Add(_source);

            var animation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(1)));
            animation.BeginAnimation(ConnectedVisual.ProgressProperty, animation);

            return true;
        }

        private class ConnectedVisual : DrawingVisual
        {
            public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
                "Progress", typeof(double), typeof(ConnectedVisual),
                new PropertyMetadata(0.0, OnProgressChanged), ValidateProgress);

            public double Progress
            {
                get => (double) GetValue(ProgressProperty);
                set => SetValue(ProgressProperty, value);
            }

            private static bool ValidateProgress(object value) =>
                value is double progress && progress >= 0 && progress <= 1;

            private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ((ConnectedVisual) d).Render((double) e.NewValue);
            }

            public ConnectedVisual([NotNull] Visual source, [NotNull] Visual destination)
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }
                if (destination == null)
                {
                    throw new ArgumentNullException(nameof(destination));
                }

                _sourceBrush = new BitmapCacheBrush(source);
                _destinationBrush = new BitmapCacheBrush(destination);
            }

            private readonly Visual _source;
            private readonly Visual _destination;
            private readonly BitmapCacheBrush _sourceBrush;
            private readonly BitmapCacheBrush _destinationBrush;
            private Rect _sourceBounds;
            private Rect _destinationBounds;

            protected override void OnVisualParentChanged(DependencyObject oldParent)
            {
                var sourceBounds = VisualTreeHelper.GetContentBounds(_source);
                _sourceBounds = new Rect(_source.PointToScreen(new Point()), _source.PointToScreen(new Point()));
            }

            private void Render(double progress)
            {
                using (var dc = RenderOpen())
                {
                    dc.PushOpacity(1 - progress);
                    dc.DrawRectangle(_sourceBrush, null, new Rect(0, 0, 100, 100));
                    dc.Pop();
                    dc.PushOpacity(progress);
                    dc.DrawRectangle(_destinationBrush, null, new Rect(0, 0, 100, 100));
                    dc.Pop();
                }
            }
        }

        private class ConnectedAnimationAdorner : Adorner
        {
            private ConnectedAnimationAdorner(AdornerLayer layer, [NotNull] UIElement adornedElement)
                : base(adornedElement)
            {
                Layer = layer;
                Children = new VisualCollection(this);
            }

            internal AdornerLayer Layer { get; }

            internal VisualCollection Children { get; }

            protected override int VisualChildrenCount => Children.Count;

            protected override Visual GetVisualChild(int index) => Children[index];

            protected override Size ArrangeOverride(Size finalSize)
            {
                foreach (var child in Children.OfType<UIElement>())
                {
                    child.Arrange(new Rect(child.DesiredSize));
                }
                return finalSize;
            }

            internal static ConnectedAnimationAdorner FindFrom([NotNull] Visual visual)
            {
                if (Window.GetWindow(visual)?.Content is UIElement root)
                {
                    var layer = AdornerLayer.GetAdornerLayer(root);
                    if (layer != null)
                    {
                        var adorner = layer.GetAdorners(root)?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                        if (adorner == null)
                        {
                            adorner = new ConnectedAnimationAdorner(layer, root);
                            layer.Add(adorner);
                        }
                        return adorner;
                    }
                }
                throw new InvalidOperationException("指定的 Visual 尚未连接到可见的视觉树中，找不到用于承载动画的容器。");
            }

            internal static void ClearFor([NotNull] Visual visual)
            {
                if (Window.GetWindow(visual)?.Content is UIElement root)
                {
                    var layer = AdornerLayer.GetAdornerLayer(root);
                    var adorner = layer?.GetAdorners(root)?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                    if (adorner != null)
                    {
                        layer.Remove(adorner);
                    }
                }
            }
        }
    }
}
