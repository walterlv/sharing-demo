using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Walterlv.Annotations;

namespace Walterlv.Demo
{
    public static class UIDispatcher
    {
        public static async Task<Dispatcher> RunNewAsync(string name = null)
        {
            return await Task.Run(() => RunNew(name));
        }

        private static Dispatcher RunNew([CanBeNull] string name = null)
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
