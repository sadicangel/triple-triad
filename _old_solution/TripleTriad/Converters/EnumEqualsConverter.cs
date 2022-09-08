namespace TripleTriad.Converters;

public sealed class EnumEqualsConverter : MarkupConverter<EnumEqualsConverter>
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum @enum && Enum.TryParse(@enum.GetType(), parameter as string, out var other))
            return @enum.Equals(other);
        return base.Convert(value, targetType, parameter, language);
    }
}