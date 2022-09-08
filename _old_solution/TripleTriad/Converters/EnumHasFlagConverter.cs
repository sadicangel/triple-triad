using System.Diagnostics.CodeAnalysis;

namespace TripleTriad.Converters;

public sealed class EnumHasFlagConverter : MarkupConverter<EnumHasFlagConverter>
{
    [SuppressMessage("Usage", "CA2248:Provide correct 'enum' argument to 'Enum.HasFlag'")]
    public override object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum @enum && Enum.TryParse(@enum.GetType(), parameter as string, out var flag))
            return @enum.HasFlag((Enum)flag!);
        return base.Convert(value, targetType, parameter, language);
    }
}