using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace TripleTriad.Converters;

public abstract class MarkupConverter<T> : MarkupExtension, IValueConverter
    where T : notnull, new()
{
    public static T Instance { get; } = new T();

    public virtual object Convert(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }

    protected override object ProvideValue() => Instance;
}
