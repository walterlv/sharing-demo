using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Walterlv.Demo.Interop
{
    public class HwndAsyncSource : HwndSource
    {
        #region Fields

        private IntPtr x;
        private IntPtr y;

        #endregion


        #region Constructors

        [SecurityCritical]
        public HwndAsyncSource(FrameworkElement root, string name = null)
            : this(root, 0x40000000, name)
        {
        }

        [SecurityCritical]
        public HwndAsyncSource(FrameworkElement root, int style, string name = null)
            : base(new HwndSourceParameters(name ?? "HwndAsyncSourceWindow")
            {
                ParentWindow = new IntPtr(-3),
                WindowStyle = style
            })

        {
            RootVisual    = root;
            SizeToContent = SizeToContent.Manual;

            if (null != CompositionTarget)
                CompositionTarget.BackgroundColor = Colors.White;

            //AddHook(HwndSourceHook);
        }


        // TODO: Add relevant constructors
        /*
       [SecurityCritical, UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        public HwndSource(HwndSourceParameters parameters)
        {
            this.Initialize(parameters);
        }
        
        [SecurityCritical]
        public HwndSource(int classStyle, int style, int exStyle, int x, int y, string name, IntPtr parent)
        {
            SecurityHelper.DemandUIWindowPermission();
            HwndSourceParameters parameters = new HwndSourceParameters(name) {
                WindowClassStyle = classStyle,
                WindowStyle = style,
                ExtendedWindowStyle = exStyle
            };
            parameters.SetPosition(x, y);
            parameters.ParentWindow = parent;
            this.Initialize(parameters);
        }
        
        [SecurityCritical]
        public HwndSource(int classStyle, int style, int exStyle, int x, int y, int width, int height, string name, IntPtr parent)
        {
            SecurityHelper.DemandUIWindowPermission();
            HwndSourceParameters parameters = new HwndSourceParameters(name, width, height) {
                WindowClassStyle = classStyle,
                WindowStyle = style,
                ExtendedWindowStyle = exStyle
            };
            parameters.SetPosition(x, y);
            parameters.ParentWindow = parent;
            this.Initialize(parameters);
        }
        
        [SecurityCritical]
        public HwndSource(int classStyle, int style, int exStyle, int x, int y, int width, int height, string name, IntPtr parent, bool adjustSizingForNonClientArea)
        {
            SecurityHelper.DemandUIWindowPermission();
            HwndSourceParameters parameters = new HwndSourceParameters(name, width, height) {
                WindowClassStyle = classStyle,
                WindowStyle = style,
                ExtendedWindowStyle = exStyle
            };
            parameters.SetPosition(x, y);
            parameters.ParentWindow = parent;
            parameters.AdjustSizingForNonClientArea = adjustSizingForNonClientArea;
            this.Initialize(parameters);
        }
         */



        #endregion


        #region Message Pump

        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x9000:
                    x = wParam;
                    y = lParam;
                    break;

                case 0x9001:
                    SetWindowPos(new HandleRef(null, Handle), new HandleRef(null, IntPtr.Zero),
                        (int)x, (int)y, (int)wParam, (int)lParam, 0x4114);
                    break;

                case 0x9002:
                    ShowWindowAsync(new HandleRef(null, Handle), (int)wParam);
                    break;

                case 0x9003:
                    EnableWindow(new HandleRef(null, Handle), IntPtr.Zero != wParam);
                    break;

                case 0x9004:
                    break;


                case 0x9005:
                    SetParent(new HandleRef(null, Handle), new HandleRef(null, wParam));
                    break;

                default:
                    return IntPtr.Zero;
            }

            handled = true;
            return IntPtr.Zero;
        }

        #endregion


        #region HwndSource
        
        
        
        
        #endregion


        #region Native Methods

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool EnableWindow(HandleRef hWnd, bool enable);

        #endregion

    }
}
