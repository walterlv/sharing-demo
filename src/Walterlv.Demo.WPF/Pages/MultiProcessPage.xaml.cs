using System;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Walterlv.Demo.Contracts;
using Walterlv.Demo.Interop;
using Path = System.IO.Path;

namespace Walterlv.Demo.Pages
{
    public partial class MultiProcessPage : Page
    {
        public MultiProcessPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                ?? Environment.CurrentDirectory;
#if DEBUG
            var path = Path.Combine(currentFolder, "..", "..", "..",
                "Extensions", "Walterlv.Extensions.WPF", "bin", "Debug");
            path = Path.GetFullPath(path);
#else
            var path = currentFolder;
#endif

            var handleContract = FrameworkElementAsyncAdapters.ViewToContractAdapter(
                new Rectangle
                {
                    Width = 200,
                    Height = 100,
                    Fill = Brushes.ForestGreen,
                });
            
            var element = FrameworkElementAsyncAdapters.ContractToViewAdapter(handleContract);
            Content = element;
        }
    }
}
