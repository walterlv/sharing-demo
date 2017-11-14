using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Walterlv.Demo
{
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