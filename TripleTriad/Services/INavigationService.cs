using Microsoft.UI.Xaml.Controls;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.Services;

public interface INavigationService
{
    void InitializeFrame(Frame rootFrame);
    void NavigateTo<T>() where T : BaseViewModel;
    void NavigateTo<T>(object parameter) where T : BaseViewModel;
    void RemoveFromBackStack();
}
