using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TripleTriad.ViewModels.Explicit;

public abstract class BaseViewModel : ObservableObject
{
    private bool _isDirty;
    private bool _isBusy;

    public bool IsDirty { get => _isDirty; protected set => SetProperty(ref _isDirty, value, OnIsDirtyChanged); }

    public bool IsBusy { get => _isBusy; protected set => SetProperty(ref _isBusy, value, OnIsBusyChanged); }

    protected bool SetProperty<T>(ref T field, T value, Action<T> callback, [CallerMemberName] string propertyName = "")
    {
        var result = SetProperty(ref field, value, propertyName);
        if (result)
            callback.Invoke(field);
        return result;
    }

    protected bool SetPropertyNotNull<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        return SetProperty(ref field, value, propertyName);
    }

    protected bool SetPropertyNotNull<T>(ref T field, T value, Action<T> callback, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        var result = SetProperty(ref field, value, propertyName);
        if (result)
            callback.Invoke(field);
        return result;
    }

    protected virtual void OnIsDirtyChanged(bool isDirty) { }

    protected virtual void OnIsBusyChanged(bool isBusy) { }
}