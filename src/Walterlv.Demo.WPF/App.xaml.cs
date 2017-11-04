using System.Windows;
using System.Windows.Controls;

namespace Walterlv.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    Frame rootFrame = CoreWindow.Current.Content as Frame;

        //    // 不要在窗口已包含内容时重复应用程序初始化，
        //    // 只需确保窗口处于活动状态
        //    if (rootFrame == null)
        //    {
        //        // 创建要充当导航上下文的框架，并导航到第一页
        //        rootFrame = new Frame();

        //        // 将框架放在当前窗口中
        //        CoreWindow.Current.Content = rootFrame;
        //    }

        //    if (rootFrame.Content == null)
        //    {
        //        // 当导航堆栈尚未还原时，导航到第一页，
        //        // 并通过将所需信息作为导航参数传入来配置
        //        // 参数
        //        rootFrame.Navigate(new MainPage(), e.Args);
        //    }
        //    // 确保当前窗口处于活动状态
        //    CoreWindow.Current.Activate();
        //}
    }
}
