using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
// ReSharper disable UnusedMember.Local

namespace Walterlv.Demo
{
    public static class WindowIconExtensions
    {
        private const int GwlExstyle = -20;
        private const int SwpFramechanged = 0x0020;
        private const int SwpNomove = 0x0002;
        private const int SwpNosize = 0x0001;
        private const int SwpNozorder = 0x0004;
        private const int WsExDlgmodalframe = 0x0001;

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.RegisterAttached("ShowIcon", typeof(bool), typeof(WindowIconExtensions),
                new FrameworkPropertyMetadata(true, OnShowIconPropertyChanged));

        public static bool GetShowIcon(UIElement element)
        {
            return (bool)element.GetValue(ShowIconProperty);
        }

        public static void SetShowIcon(UIElement element, bool value)
        {
            element.SetValue(ShowIconProperty, value);
        }

        private static void OnShowIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (false.Equals(e.NewValue) && d is Window window)
            {
                RemoveIcon(window);
            }
        }

        public static void RemoveIcon(this Window window)
        {
            if (window.IsInitialized)
            {
                OnSourceInitialized(null, null);
            }
            else
            {
                window.SourceInitialized += OnSourceInitialized;
            }

            void OnSourceInitialized(object sender, EventArgs eventArgs)
            {
                window.SourceInitialized -= OnSourceInitialized;
                var hwnd = new WindowInteropHelper(window).Handle;
                var extendedStyle = GetWindowLong(hwnd, GwlExstyle);
                SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExDlgmodalframe);
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNozorder | SwpFramechanged);
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint msg,
            IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
            int x, int y, int width, int height, uint flags);
    }
}
