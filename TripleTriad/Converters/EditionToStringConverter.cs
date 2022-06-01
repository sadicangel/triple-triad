namespace TripleTriad.Converters;

public sealed class EditionToStringConverter : MarkupConverter<EditionToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            1 => "FFVIII",
            _ => base.Convert(value, targetType, parameter, language)
        };
    }

    public override object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            "FFVIII" => 1,
            _ => base.Convert(value, targetType, parameter, language)
        };
    }
}