using System.AddIn.Contract;
using System.Security;
using System.Windows;
using Walterlv.Demo.Contracts;

namespace Walterlv.Demo.Interop
{
    //
    // Summary:
    //     System.AddIn.Pipeline.FrameworkElementAdapters is used by Windows Presentation
    //     Foundation (WPF) add-ins to convert between a System.Windows.FrameworkElement
    //     and an System.AddIn.Contract.INativeHandleContract.
    public static class FrameworkElementAsyncAdapters
    {
        //
        // Summary:
        //     Returns a System.Windows.FrameworkElement that has been converted from an System.AddIn.Contract.INativeHandleContract.
        //
        // Parameters:
        //   nativeHandleContract:
        //     The System.AddIn.Contract.INativeHandleContract that was passed across the isolation
        //     boundary between the host application and the add-in.
        //
        // Returns:
        //     A System.Windows.FrameworkElement that will be displayed from either the host
        //     application or add-in, depending on the direction in which the UI is passed between
        //     the two.
        [SecurityCritical]
        public static FrameworkElement ContractToViewAdapter(INativeHandleContract nativeHandleContract)
        {
            NativeHandleHostAdapter host = null;
            if (nativeHandleContract != null)
            {
                host = new NativeHandleHostAdapter(nativeHandleContract);
            }
            return host;
        }



        /// <summary>
        /// Returns an System.AddIn.Contract.INativeHandleContract that has been converted 
        /// from a System.Windows.FrameworkElement.
        /// </summary>
        /// <param name="root">The System.Windows.FrameworkElement to be passed across the 
        /// isolation boundary between the host application and the add-in.</param>
        /// <returns>An System.AddIn.Contract.INativeHandleContract that is passed from 
        /// either the host application or the add-in, depending on the direction in which 
        /// the UI is passed between the two.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">root is not the root element
        /// of a tree of elements.</exception>
        [SecurityCritical]
        public static INativeHandleContract ViewToContractAdapter(FrameworkElement root)
        {
            return new NativeHandleAddInAdapter(root);
        }
    }
}