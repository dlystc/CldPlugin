using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;
using UMP.DlyStc.Plugin.Cld.Components.Stc;

namespace UMP.DlyStc.Plugin.Cld.Shared.Converters;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");
        return !(bool)value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class IntToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return System.Convert.ToString((int)value!);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return (string)value! == "" ? 0 : System.Convert.ToInt32((string)value!);
    }
}

public class DoubleToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return System.Convert.ToString((double)value!, CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        try
        {
            return System.Convert.ToDouble((string)value!);
        }
        catch
        {
            return 0;
        }
    }
}

public class EnumDescriptionConverter : IValueConverter
{
    object IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Enum myEnum = (Enum)value!;

        string description = GetEnumDescription(myEnum);

        return description;
    }

    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Empty;
    }

    static string GetEnumDescription(Enum? enumObj)
    {
        if (enumObj == null) return string.Empty;
        
        var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
        if (fieldInfo == null) return enumObj.ToString();

        var attribArray = fieldInfo.GetCustomAttributes(false);
        if (attribArray.Length == 0) return enumObj.ToString();

        if (attribArray[0] is DescriptionAttribute attrib)
            return attrib.Description;

        return enumObj.ToString();
    }
}

public class SeparateAttributesDisplayVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return (StcConfig.AttributesDisplayRule)value! switch
        {
            StcConfig.AttributesDisplayRule.Separate => true,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}