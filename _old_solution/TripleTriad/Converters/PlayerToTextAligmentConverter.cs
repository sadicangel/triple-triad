using Microsoft.UI.Xaml;
using TripleTriad.Models;

namespace TripleTriad.Converters;

public sealed class PlayerToTextAligmentConverter : MarkupConverter<PlayerToTextAligmentConverter>
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Player player)
            return player.IsLeft ? TextAlignment.Left : TextAlignment.Right;
        return TextAlignment.Center;
    }
}