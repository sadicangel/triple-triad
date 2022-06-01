using Microsoft.UI.Xaml;
using TripleTriad.Services;
using TripleTriad.ViewModels;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TripleTriad;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow(INavigationService navigation)
    {
        InitializeComponent();
        navigation.InitializeFrame(RootFrame);
    }
}