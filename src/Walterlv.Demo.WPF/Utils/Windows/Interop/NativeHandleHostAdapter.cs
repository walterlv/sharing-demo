using System;
using System.AddIn.Contract;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Walterlv.Demo.Contracts;

namespace Walterlv.Demo.Interop
{
    public class NativeHandleHostAdapter : FrameworkElement, IWin32Window, 
                                                             IKeyboardInputSink, 
                                                             IDisposable
    {
        #region Fields

        private Rect location;
        private HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
        private bool isBuildingWindow;
        private PresentationSource source;
        private ContractHandle contract;

        #endregion


        #region Constructors

        static NativeHandleHostAdapter() 
        {
            Control.IsTabStopProperty.OverrideMetadata(typeof(NativeHandleHostAdapter), new FrameworkPropertyMetadata(true));
            FocusableProperty.OverrideMetadata(typeof(NativeHandleHostAdapter), new FrameworkPropertyMetadata(true));
        }

        public NativeHandleHostAdapter(INativeHandleContract nativeHandleContract/*, ObserverHostAdapter adapter*/)
        {
            Debug.Assert(null != nativeHandleContract);
            contract = new ContractHandle(nativeHandleContract);

            //observer = adapter;

            //PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
            Dispatcher.ShutdownFinished += OnDispatcherShutdown;

            //if (null == observer) return;
            //observer.Execute += ExecutedRoutedCommandHandler;
            //observer.CanExecute += CanExecuteRoutedCommandHandler;
        }

        #endregion


        #region Public Properties

        public IntPtr Handle { get { return hWnd.Handle; } }
        
        public IDisposable Observer { get; set; }

        #endregion


        #region FrameworkElement



        #endregion


        #region Implementation

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (IntPtr.Zero == Handle) 
                return;

            CompositionTarget compositionTarget = null;
            if ((Handle != IntPtr.Zero) && IsVisible && source != null)
            {
                compositionTarget = source.CompositionTarget;
            }

            if ((compositionTarget != null) && (compositionTarget.RootVisual != null))
            {
                var rect = RootToClient(ElementToRoot(new Rect(RenderSize), this, source), source);
            

                // TODO: Refactoring
                if (Rect.Equals(location, rect))
                    return;

                location = rect;

                PostMessage(new HandleRef(null, Handle), 0x9000, (IntPtr)rect.X, (IntPtr)rect.Y);
                PostMessage(new HandleRef(null, Handle), 0x9001, (IntPtr)rect.Width, (IntPtr)rect.Height);

                PostMessage(new HandleRef(null, Handle), 0x9002, (IntPtr)5, IntPtr.Zero);
            }
            else
            {
                PostMessage(new HandleRef(null, Handle), 0x9002, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var value = (false == (bool) e.NewValue) ? IntPtr.Zero : (IntPtr) 1;
            PostMessage(new HandleRef(null, Handle), 0x9003, value, IntPtr.Zero);
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue ? (IntPtr)8 : IntPtr.Zero;
            PostMessage(new HandleRef(null, Handle), 0x9002, value, IntPtr.Zero);
        }

        private void OnSourceChanged(object sender, SourceChangedEventArgs e)
        {
            IKeyboardInputSite keyboardInputSite = ((IKeyboardInputSink)this).KeyboardInputSite;
            if (keyboardInputSite != null)
            {
                ((IKeyboardInputSink)this).KeyboardInputSite = null;
                keyboardInputSite.Unregister();
            }
            IKeyboardInputSink sink = source as IKeyboardInputSink;
            if (sink != null)
            {
                ((IKeyboardInputSink)this).KeyboardInputSite = sink.RegisterKeyboardInputSink(this);
            }

            source = e.NewSource;

            if (!isBuildingWindow)
            {
                isBuildingWindow = true;
                HwndSource parent = (HwndSource)e.NewSource;

                try
                {
                    if (hWnd.Handle == IntPtr.Zero)
                    {
                        hWnd = new HandleRef(this, ((INativeHandleContract)contract.Contract).GetHandle());

                        LayoutUpdated += OnLayoutUpdated;
                        IsEnabledChanged += OnEnabledChanged;
                        IsVisibleChanged += OnVisibleChanged;

                        PostMessage(new HandleRef(null, Handle), 0x9005, parent.Handle, IntPtr.Zero);
                        PostMessage(new HandleRef(null, Handle), 0x9002, IntPtr.Zero, IntPtr.Zero);
                        InvalidateMeasure();
                    }
                }
                finally
                {
                    isBuildingWindow = false;
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        private void OnDispatcherShutdown(object sender, EventArgs e)
        {
            Dispose();
        }

        bool CanExecuteRoutedCommandHandler(RoutedCommand command, object parameter)
        {
            return command.CanExecute(parameter, this);
        }

        void ExecutedRoutedCommandHandler(RoutedCommand command, object parameter)
        {
            command.Execute(parameter, this);
        }

        static Rect ElementToRoot(Rect rectElement, Visual element, PresentationSource presentationSource)
        {
            return element.TransformToAncestor(presentationSource.RootVisual).TransformBounds(rectElement);
        }

        static Rect RootToClient(Rect rectRoot, PresentationSource presentationSource)
        {
            var compositionTarget = presentationSource.CompositionTarget;
            Debug.Assert(compositionTarget != null, "Composition Target Invalid");
            var visualTransform = GetVisualTransform(compositionTarget.RootVisual);
            var rect = Rect.Transform(rectRoot, visualTransform);
            var transformToDevice = compositionTarget.TransformToDevice;
            return Rect.Transform(rect, transformToDevice);
        }

        static Matrix GetVisualTransform(Visual v)
        {
            if (v == null)
            {
                return Matrix.Identity;
            }
            var identity = Matrix.Identity;
            Transform transform = VisualTreeHelper.GetTransform(v);
            if (transform != null)
            {
                Matrix matrix2 = transform.Value;
                identity = Matrix.Multiply(identity, matrix2);
            }
            var offset = VisualTreeHelper.GetOffset(v);
            identity.Translate(offset.X, offset.Y);
            return identity;
        }

        #endregion


        #region IKeyboardInputSink

        public bool HasFocusWithin()
        {
            HandleRef handleRef = new HandleRef(this, GetFocus());
            if (!(hWnd.Handle != IntPtr.Zero) || (!(handleRef.Handle == hWnd.Handle) &&
                !IsChild(hWnd, handleRef)))
            {
                return false;
            }
            return true;
        }

        public IKeyboardInputSite KeyboardInputSite { get; set; }

        public bool OnMnemonic(ref MSG msg, ModifierKeys modifiers)
        {
            return false;
        }

        public IKeyboardInputSite RegisterKeyboardInputSink(IKeyboardInputSink sink)
        {
            throw new InvalidOperationException("HwndHostDoesNotSupportChildKeyboardSinks");
        }

        public bool TabInto(TraversalRequest request)
        {
            return false;
        }

        public bool TranslateAccelerator(ref MSG msg, ModifierKeys modifiers)
        {
            return false;
        }

        public bool TranslateChar(ref MSG msg, ModifierKeys modifiers)
        {
            return false;
        }

        #endregion


        #region Native Methods

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetFocus();

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool IsChild(HandleRef hWndParent, HandleRef hwnd);

        #endregion


        #region IDisposable

        ~NativeHandleHostAdapter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            PresentationSource.RemoveSourceChangedHandler(this, OnSourceChanged);
            Dispatcher.ShutdownFinished -= OnDispatcherShutdown;

            //if (null != observer)
            //{
            //    //    observer.Execute -= ExecutedRoutedCommandHandler;
            //    //    observer.CanExecute -= CanExecuteRoutedCommandHandler;
            //    observer = null;
            //}

            if (null != contract)
            {
                contract.Dispose();
                contract = null;
            }

            hWnd = new HandleRef(null, IntPtr.Zero);
        }

        #endregion


        #region Nested Types

        #endregion
    }
}
