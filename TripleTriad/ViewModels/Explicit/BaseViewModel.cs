using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TripleTriad.ViewModels.Explicit;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> EventArgs = new();

    private bool _isDirty;

    public bool IsDirty { get => _isDirty; protected set => SetProperty(ref _isDirty, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected SetPropertyResult<TValue> SetProperty<TValue>(ref TValue field, TValue value, [CallerMemberName] string propertyName = "")
    {
        var areEqual = EqualityComparer<TValue>.Default.Equals(field, value);
        if (!areEqual)
        {
            field = value;
            NotifyPropertyChanged(propertyName);
        }
        return new SetPropertyResult<TValue> { Changed = !areEqual, Value = field };
    }

    protected SetPropertyResult<TValue> SetPropertyNotNull<TValue>(ref TValue field, TValue value, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        return SetProperty(ref field, value, propertyName);
    }

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, EventArgs.GetOrAdd(propertyName, pn => new PropertyChangedEventArgs(pn)));
    }
}