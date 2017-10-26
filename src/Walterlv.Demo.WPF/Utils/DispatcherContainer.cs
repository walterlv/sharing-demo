using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Walterlv.Annotations;

namespace Walterlv.Demo
{
    [ContentProperty(nameof(Child))]
    public class DispatcherContainer : FrameworkElement
    {
        public DispatcherContainer()
        {
            _hostVisual = new HostVisual();
            AddVisualChild(_hostVisual);
        }

        private UIElement _child;
        private readonly HostVisual _hostVisual;
        private VisualTarget _visualTarget;

        public UIElement Child
        {
            get => _child;
            set
            {
                if (_child != null)
                {
                    _visualTarget?.Dispatcher.Invoke(() =>
                    {
                        _visualTarget.Dispose();
                    });
                }

                _child = value;

                if (_child == null)
                {
                    _visualTarget = null;
                }
                else
                {
                    value.Dispatcher.Invoke(() =>
                    {
                        _visualTarget = new VisualTarget(_hostVisual)
                        {
                            RootVisual = value,
                        };
                    });

                    InvalidateMeasure();
                    InvalidateArrange();
                }
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _hostVisual;
        }

        protected override int VisualChildrenCount => _child != null ? 1 : 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            var child = Child;
            if (child == null)
                return default(Size);

            var desiredSize = default(Size);
            child.Dispatcher.InvokeAsync(
                () => child.Measure(availableSize),
                DispatcherPriority.Loaded);
            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var child = Child;
            if (child == null)
                return finalSize;

            child.Dispatcher.InvokeAsync(
                () => child.Arrange(new Rect(finalSize)),
                DispatcherPriority.Loaded);
            return finalSize;
        }

        public static async Task<T> CreateUIElementAsync<T>([NotNull] Func<T> @new, Dispatcher dispatcher = null)
            where T : UIElement
        {
            if (@new == null)
                throw new ArgumentNullException(nameof(@new));

            var originDispatcher = Dispatcher.CurrentDispatcher;
            var element = default(T);
            if (dispatcher == null)
            {
                var resetEvent = new ManualResetEvent(false);
                var thread = new Thread(() =>
                {
                    try
                    {
                        element = @new();
                        resetEvent.Set();
                        Dispatcher.Run();
                    }
                    catch (Exception ex)
                    {
                        originDispatcher.InvokeAsync(() =>
                        {
                            ExceptionDispatchInfo.Capture(ex).Throw();
                        });
                    }
                })
                {
                    Name = $"{typeof(T).Name}",
                    IsBackground = true,
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                await Task.Run(() => resetEvent.WaitOne())
                    .ContinueWith(x => resetEvent.Dispose());
            }
            else
            {
                await dispatcher.InvokeAsync(() =>
                {
                    element = @new();
                });
            }
            return element;
        }
    }
}
