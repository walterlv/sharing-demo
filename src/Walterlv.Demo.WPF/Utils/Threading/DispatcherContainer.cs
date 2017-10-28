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
    public sealed class DispatcherContainer : FrameworkElement
    {
        public DispatcherContainer()
        {
            _hostVisual = new HostVisual();
        }

        private bool _isUpdatingChild;
        private readonly HostVisual _hostVisual;
        private VisualTargetPresentationSource _targetSource;

        public UIElement Child { get; private set; }

        public async Task SetChildAsync(UIElement value)
        {
            if (_isUpdatingChild)
            {
                throw new InvalidOperationException("Child property should not be set during Child updating.");
            }

            _isUpdatingChild = true;
            try
            {
                await SetChildAsync();
            }
            finally
            {
                _isUpdatingChild = false;
            }

            async Task SetChildAsync()
            {
                var oldChild = Child;
                var visualTarget = _targetSource;

                if (Equals(oldChild, value))
                    return;

                _targetSource = null;
                if (visualTarget != null)
                {
                    RemoveVisualChild(oldChild);
                    await visualTarget.Dispatcher.InvokeAsync(visualTarget.Dispose);
                }

                Child = value;

                if (value == null)
                {
                    _targetSource = null;
                }
                else
                {
                    await value.Dispatcher.InvokeAsync(() =>
                    {
                        _targetSource = new VisualTargetPresentationSource(_hostVisual)
                        {
                            RootVisual = value,
                        };
                    });
                    AddVisualChild(_hostVisual);
                }
                InvalidateMeasure();
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _hostVisual;
        }

        protected override int VisualChildrenCount => Child != null ? 1 : 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            var child = Child;
            if (child == null)
                return default(Size);

            child.Dispatcher.InvokeAsync(
                () => child.Measure(availableSize),
                DispatcherPriority.Loaded);

            return default(Size);
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

        public static async Task<T> CreateElementAsync<T>(Dispatcher dispatcher = null)
            where T : Visual, new()
        {
            return await CreateElementAsync(() => new T(), dispatcher);
        }

        public static async Task<T> CreateElementAsync<T>(
            [NotNull] Func<T> @new, Dispatcher dispatcher = null)
            where T : Visual
        {
            if (@new == null)
                throw new ArgumentNullException(nameof(@new));

            var element = default(T);
            if (dispatcher == null)
            {
                Exception exception = null;
                var resetEvent = new AutoResetEvent(false);
                var thread = new Thread(() =>
                {
                    try
                    {
                        SynchronizationContext.SetSynchronizationContext(
                            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                        element = @new();
                        resetEvent.Set();
                        Dispatcher.Run();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                })
                {
                    Name = $"{typeof(T).Name}",
                    IsBackground = true,
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                await Task.Run(() =>
                {
                    resetEvent.WaitOne();
                    resetEvent.Dispose();
                });
                if (exception != null)
                {
                    ExceptionDispatchInfo.Capture(exception).Throw();
                }
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
