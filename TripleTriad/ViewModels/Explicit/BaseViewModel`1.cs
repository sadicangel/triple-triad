using System.Runtime.CompilerServices;

namespace TripleTriad.ViewModels.Explicit;

public abstract class BaseViewModel<TModel> : BaseViewModel where TModel : new()
{
    private TModel _model = new();
    public TModel Model { get => _model; set => SetPropertyNotNull(ref _model, value, OnModelChanged0); }

    protected bool SetProperty<TValue>(Func<TModel, TValue> get, Action<TModel, TValue> set, TValue value, Action<TValue>? callback = null, [CallerMemberName] string propertyName = "")
    {
        var field = get.Invoke(Model);
        var areEqual = EqualityComparer<TValue>.Default.Equals(field, value);
        if (!areEqual)
        {
            set.Invoke(Model, value);
            OnPropertyChanged(propertyName);
            callback?.Invoke(get.Invoke(Model));
            return true;
        }
        return false;
    }

    protected bool SetPropertyNotNull<TValue>(Func<TModel, TValue> get, Action<TModel, TValue> set, TValue value, Action<TValue>? callback = null, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        return SetProperty(get, set, value, callback, propertyName);
    }

    private void OnModelChanged0(TModel model)
    {
        OnModelChanged(model);
        OnPropertyChanged();
    }

    protected virtual void OnModelChanged(TModel model)
    {

    }
}