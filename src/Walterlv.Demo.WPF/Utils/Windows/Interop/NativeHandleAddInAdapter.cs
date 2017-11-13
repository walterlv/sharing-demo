using System;
using System.AddIn.Contract;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Walterlv.Demo.Contracts;

namespace Walterlv.Demo.Interop
{
    public class NativeHandleAddInAdapter : ContractBase, INativeHandleContract
    {
        #region Fields

        private HwndSource hwndSource;

        #endregion


        #region Constructors


        public NativeHandleAddInAdapter(FrameworkElement root, string name = null)
        {
            Debug.Assert(null != root);
            root.VerifyAccess();

            hwndSource = new HwndAsyncSource(root, name);

            hwndSource.AddHook(HwndSourceHook);
            
            CommandManager.AddCanExecuteHandler(root, CanExecuteRoutedEventHandler);
            CommandManager.AddExecutedHandler(root, ExecuteRoutedEventHandler);

            // TODO: Refactoring
            Debug.WriteLine("--\t\t\t\t\t\t\t Handle: {0}\t--\tWindow: {1}", hwndSource.Handle.ToString("x8"), name);
        }


        #endregion

        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // TODO: Refactoring
            var name = Enum.GetName(typeof(WindowMessage), msg);
            Debug.WriteLine("--\tTicks: {0} \t Handle: {1} \t Message: {2} \t wParam: {3} \t lParam: {4}",
                Stopwatch.GetTimestamp(), 
                hwnd.ToString("x8"), name ?? msg.ToString("x4"), wParam.ToString("x8"), lParam.ToString("x8"));

            return IntPtr.Zero;
        }


        #region RoutedCommands

        public void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            bool isSerializable = (null == e.Parameter) || e.Parameter.GetType().IsSerializable;
            if (!(e.Command is RoutedCommand) || !isSerializable)
                return;

            e.Handled = true;
        }

        public void ExecuteRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            bool isSerializable = (null == e.Parameter) || e.Parameter.GetType().IsSerializable;
            if (!(e.Command is RoutedCommand) || !isSerializable)
                return;
        }

        #endregion


        #region Implementation



        protected override void OnFinalRevoke()
        {
            if (null != hwndSource)
            {
                CommandManager.RemoveCanExecuteHandler((UIElement)hwndSource.RootVisual, CanExecuteRoutedEventHandler);
                CommandManager.RemoveExecutedHandler((UIElement)hwndSource.RootVisual, ExecuteRoutedEventHandler);
                
                if (!hwndSource.CheckAccess())
                    hwndSource.Dispatcher.Invoke(new Action(hwndSource.Dispose));
                else
                    hwndSource.Dispose();
                hwndSource = null;
            }
            base.OnFinalRevoke();
        }

        #endregion


        #region INativeHandleContract

        public IntPtr GetHandle()
        {
            return hwndSource.Handle;
        }

        #endregion

    }
}
