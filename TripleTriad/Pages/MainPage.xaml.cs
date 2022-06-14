using Microsoft.UI.Xaml.Controls;
using TripleTriad.ViewModels;

namespace TripleTriad.Pages;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();

        DataContext = ViewModel = App.GetService<MainViewModel>();
        ViewModel.View = this;
    }
}
