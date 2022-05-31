using System.Collections.Concurrent;
using TripleTriad.Models;

namespace TripleTriad.Converters;

public sealed class ElementToImageSourceConverter : MarkupConverter<ElementToImageSourceConverter>
{
    private static readonly ConcurrentDictionary<Element, Uri> Bitmaps = new();
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Element element && Enum.IsDefined(element))
            return Bitmaps.GetOrAdd(element, e => new Uri($"ms-appx://TripleTriad.Shared/Assets/E{e}.png", UriKind.RelativeOrAbsolute));
        
        return base.Convert(value, targetType, parameter, language);
    }
}
