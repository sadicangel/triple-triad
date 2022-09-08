using System.Collections.Concurrent;

namespace TripleTriad.Converters;

public sealed class NumberToImageSourceConverter : MarkupConverter<NumberToImageSourceConverter>
{
    private static readonly ConcurrentDictionary<int, Uri> Bitmaps = new();
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int number && number is >= 0 and <= 10)
            return Bitmaps.GetOrAdd(number, n => new Uri($"ms-appx://TripleTriad.Shared/Assets/N{n}.png", UriKind.RelativeOrAbsolute));
        
        return base.Convert(value, targetType, parameter, language);
    }
}
