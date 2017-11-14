using System;
using System.IO.Pipes;

namespace Walterlv.Demo
{
    public class Program
    {
        [STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static int Main(string[] args)
        {
            // 以传统方式运行 WPF 应用程序。
            return new App().Run();

            //// 以异步窗口方式运行 WPF 应用程序。
            //return new AsyncApplication<App>().Run();
        }
    }
}
