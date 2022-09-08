using Microsoft.UI.Xaml;
using TripleTriad.Models;

namespace TripleTriad.Converters;

public sealed class PlayerToHorizontalAligmentConverter : MarkupConverter<PlayerToHorizontalAligmentConverter>
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Player player)
            return player.IsLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        return HorizontalAlignment.Center;
    }
}