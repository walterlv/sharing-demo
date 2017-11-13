using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Walterlv.Demo
{
    public class Program
    {
        [STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static int Main(string[] args)
        {
            new App().Run();
            return 0;

            var watch = new Stopwatch();
            watch.Start();

            Application app = null;
            var dispatcher = UIDispatcher.RunNew();
            var mainCoreWindow = CoreWindow.Current;
            dispatcher.InvokeAsync(async () =>
            {
                var panel = new Grid();
                var h = new DispatcherContainer();
                var s = new Border {Background = Brushes.Teal};
                var w = new Window
                {
                    Content = panel,
                    WindowState = WindowState.Maximized,
                };
                panel.Children.Add(h);
                panel.Children.Add(s);
                w.Closed += (sender, eventArgs) => app?.Dispatcher.InvokeAsync(() => app?.Shutdown());
                w.ContentRendered += (sender, eventArgs) => Debug.WriteLine($"[Window Rendered] {watch.Elapsed}");
                w.Show();

                Debug.WriteLine($"[Window Shown] {watch.Elapsed}");

                mainCoreWindow.ThreadedWindowInfo = (w, h, dispatcher);

                await Task.Delay(1000);
                panel.Children.Remove(s);
            });

            Debug.WriteLine($"[App Prepaired] {watch.Elapsed}");

            app = new App();

            Debug.WriteLine($"[App Created] {watch.Elapsed}");

            var exitCode = app.Run();

            Debug.WriteLine($"[App Stopped] {watch.Elapsed}");
            watch.Stop();

            return exitCode;
        }
    }

    public class CoreWindow : DispatcherObject
    {
        private static readonly ConcurrentDictionary<Dispatcher, CoreWindow> WindowDictionary =
            new ConcurrentDictionary<Dispatcher, CoreWindow>();

        private readonly ConcurrentQueue<Delegate> _queuedActions = new ConcurrentQueue<Delegate>();

        private UIElement _content;
        private (Window Window, DispatcherContainer Host, Dispatcher Dispatcher) _threadedWindowInfo;

        public static CoreWindow Current
        {
            get
            {
                lock (WindowDictionary)
                {
                    if (!WindowDictionary.TryGetValue(Dispatcher.CurrentDispatcher, out var window))
                    {
                        window = new CoreWindow();
                        WindowDictionary[Dispatcher.CurrentDispatcher] = window;
                    }
                    return window;
                }
            }
        }

        private bool IsInitialized => ThreadedWindowInfo.Window != null;

        internal (Window Window, DispatcherContainer Host, Dispatcher Dispatcher) ThreadedWindowInfo
        {
            get => _threadedWindowInfo;
            set
            {
                _threadedWindowInfo = value;
                DoNow();
            }
        }

        public UIElement Content
        {
            get => _content;
            set
            {
                _content = value;

                DoAfterInitialized(async () =>
                {
                    await ThreadedWindowInfo.Host.SetChildAsync(value);
                });
            }
        }

        public void Activate()
        {
            DoAfterInitialized(() =>
            {
                ThreadedWindowInfo.Dispatcher.InvokeAsync(() => ThreadedWindowInfo.Window.Activate());
            });
        }

        private void DoAfterInitialized(Action action)
        {
            if (IsInitialized)
            {
                action();
            }
            else
            {
                _queuedActions.Enqueue(action);
            }
        }

        private void DoAfterInitialized(Func<Task> action)
        {
            if (IsInitialized)
            {
                action();
            }
            else
            {
                _queuedActions.Enqueue(action);
            }
        }

        private async void DoNow()
        {
            while (_queuedActions.TryDequeue(out var @delegate))
            {
                if (@delegate is Action action)
                {
                    action();
                }
                else if (@delegate is Func<Task> asyncAction)
                {
                    await asyncAction();
                }
            }
        }
    }
}
