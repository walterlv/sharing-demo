using System;
using System.Threading;
using System.Windows.Threading;
using Walterlv.Annotations;
using Walterlv.Demo.Utils.Threading;

namespace Walterlv.Demo
{
    public static class UIDispatcher
    {
        public static DispatcherAsyncOperation<Dispatcher> RunNewAsync([CanBeNull] string name = null)
        {
            var awaitable = DispatcherAsyncOperation<Dispatcher>.Create(out var reportResult);
            var thread = new Thread(() =>
            {
                try
                {
                    var dispatcher = Dispatcher.CurrentDispatcher;
                    SynchronizationContext.SetSynchronizationContext(
                        new DispatcherSynchronizationContext(dispatcher));
                    reportResult(dispatcher, null);
                    Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    reportResult(null, ex);
                }
            })
            {
                Name = name ?? "BackgroundUI",
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return awaitable;
        }
    }
}
