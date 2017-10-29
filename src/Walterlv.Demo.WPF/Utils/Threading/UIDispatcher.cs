using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Walterlv.Annotations;

namespace Walterlv.Demo
{
    public static class UIDispatcher
    {
        public static async Task<T> CreateElementAsync<T>(Dispatcher dispatcher = null)
            where T : Visual, new()
        {
            return await CreateElementAsync(() => new T(), dispatcher);
        }

        public static async Task<T> CreateElementAsync<T>(
            [NotNull] Func<T> @new, [CanBeNull] Dispatcher dispatcher = null)
            where T : Visual
        {
            if (@new == null)
                throw new ArgumentNullException(nameof(@new));

            dispatcher = dispatcher ?? await RunNewAsync($"{typeof(T).Name}");
            var element = await dispatcher.InvokeAsync(@new);
            return element;
        }

        public static async Task<Dispatcher> RunNewAsync(string name = null)
        {
            return await Task.Run(() => RunNew(name));
        }

        private static Dispatcher RunNew(string name = null)
        {
            Dispatcher dispatcher = null;
            Exception exception = null;
            var resetEvent = new AutoResetEvent(false);
            var thread = new Thread(() =>
            {
                try
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    SynchronizationContext.SetSynchronizationContext(
                        new DispatcherSynchronizationContext(dispatcher));
                    
                    // ReSharper disable once AccessToDisposedClosure
                    resetEvent.Set();
                    Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            })
            {
                Name = name ?? "BackgroundUI",
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            resetEvent.WaitOne();
            resetEvent.Dispose();
            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
            return dispatcher;
        }
    }
}
