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
            
            var domain = AppDomain.CreateDomain("X");
            var instance = (DomainX)domain.CreateInstanceAndUnwrap(
                typeof(DomainX).Assembly.FullName,
                typeof(DomainX).FullName);

            var contract = instance.GetElement();

            var element = FrameworkElementAsyncAdapters.ContractToViewAdapter(contract);
            Content = element;
        }
    }

    internal sealed class DomainX : MarshalByRefObject
    {
        public INativeHandleContract GetElement()
        {
            return FrameworkElementAsyncAdapters.ViewToContractAdapter(
                new Rectangle
                {
                    Width = 200,
                    Height = 100,
                    Fill = Brushes.ForestGreen,
                });
        }
    }
}
