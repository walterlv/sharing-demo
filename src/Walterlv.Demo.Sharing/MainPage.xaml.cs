#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace Walterlv.Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
