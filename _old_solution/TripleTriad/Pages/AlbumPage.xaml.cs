using Microsoft.UI.Xaml.Controls;
using TripleTriad.ViewModels;

namespace TripleTriad.Pages;

public sealed partial class AlbumPage : Page
{
    public AlbumViewModel ViewModel { get; }

    public AlbumPage()
    {
        InitializeComponent();
        DataContext = ViewModel = App.GetService<AlbumViewModel>();
    }
}
