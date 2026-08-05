using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Shared.Contracts.Converters
{
    public class EnumDescriptionTypeConverter<TEnum> : TypeConverter where TEnum : struct, Enum
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                // Find matching enum by [Description] attribute or Name
                foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (attribute != null && attribute.Description.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return (TEnum)field.GetValue(null)!;
                    }
                    if (field.Name.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return (TEnum)field.GetValue(null)!;
                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
