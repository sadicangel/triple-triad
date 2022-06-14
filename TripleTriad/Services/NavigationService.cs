using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using System.Reflection;
using TripleTriad.ViewModels;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.Services;

public class NavigationService : INavigationService
{
    private Frame? _shellFrame;

    public void InitializeFrame(Frame rootFrame)
    {
        _shellFrame = rootFrame;
        NavigateTo<MainViewModel>();
    }

    public void NavigateTo<T>() where T : BaseViewModel
    {
        InternalNavigateTo(typeof(T));
    }

    public void NavigateTo<T>(object? parameter) where T : BaseViewModel
    {
        InternalNavigateTo(typeof(T));
    }

    public void RemoveFromBackStack()
    {
        _shellFrame?.BackStack.Remove(_shellFrame.BackStack.Last());
    }

    private void InternalNavigateTo(Type viewModelType)
    {
        var pageType = GetPageTypeForViewModel(viewModelType);
        _shellFrame?.Navigate(pageType);
    }

    private static Type GetPageTypeForViewModel(Type viewModelType)
    {
        var viewName = viewModelType.FullName!.Replace("ViewModel", "Page");
        var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;
        var viewAssemblyName = String.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
        var viewType = Type.GetType(viewAssemblyName);
        return viewType!;
    }
}