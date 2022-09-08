using Microsoft.UI.Xaml.Media;
using TripleTriad.Extensions;
using Windows.UI;

namespace TripleTriad.Converters;

public sealed class ArgbToBrushConverter : MarkupConverter<ArgbToBrushConverter>
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            uint argb => new SolidColorBrush(argb.ToColor()),
            _ => base.Convert(value, targetType, parameter, language),
        };
    }

    public override object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if(value is SolidColorBrush brush)
        {
            if (targetType == typeof(uint))
                return brush.Color.ToUint32();
        }
        return base.ConvertBack(value, targetType, parameter, language);
    }
}