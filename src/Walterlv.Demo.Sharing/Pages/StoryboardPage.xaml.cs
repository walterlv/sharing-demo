#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace Walterlv.Demo.Pages
{
    public partial class StoryboardPage : Page
    {
        public StoryboardPage()
        {
            InitializeComponent();
        }
    }
}
