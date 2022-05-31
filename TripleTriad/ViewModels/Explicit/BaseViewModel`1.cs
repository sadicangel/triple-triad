using System.Runtime.CompilerServices;

namespace TripleTriad.ViewModels.Explicit;

public abstract class BaseViewModel<TModel> : BaseViewModel where TModel : new()
{
    private TModel _model = new();
    public TModel Model { get => _model; set => SetPropertyNotNull(ref _model, value).Then(OnModelChanged0); }

    protected SetPropertyResult<TValue> SetProperty<TValue>(Func<TModel, TValue> get, Action<TModel, TValue> set, TValue value, [CallerMemberName] string propertyName = "")
    {
        var field = get.Invoke(Model);
        var areEqual = EqualityComparer<TValue>.Default.Equals(field, value);
        if (!areEqual)
        {
            set.Invoke(Model, value);
            NotifyPropertyChanged(propertyName);
        }
        return new SetPropertyResult<TValue> { Changed = !areEqual, Value = field };
    }

    protected SetPropertyResult<TValue> SetPropertyNotNull<TValue>(Func<TModel, TValue> get, Action<TModel, TValue> set, TValue value, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        return SetProperty(get, set, value, propertyName);
    }

    private void OnModelChanged0(TModel model)
    {
        OnModelChanged(model);
        NotifyPropertyChanged();
    }

    protected virtual void OnModelChanged(TModel model)
    {

    }
}