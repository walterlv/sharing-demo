using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Walterlv.Demo
{
    public class AsyncApplication<T> where T : Application, new()
    {
        public int Run()
        {
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

            app = new T();

            Debug.WriteLine($"[App Created] {watch.Elapsed}");

            var exitCode = app.Run();

            Debug.WriteLine($"[App Stopped] {watch.Elapsed}");
            watch.Stop();

            return exitCode;
        }
    }
}
