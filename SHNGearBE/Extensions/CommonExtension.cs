using System;
using System.ComponentModel;
using System.Reflection;

namespace SHNGearBE.Extensions;

public static class CommonExtension
{
    public static string GetDescription(this System.Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    // public static string GetDescription(this Enum value)
    // {
    //     var fieldInfo = value.GetType().GetField(value.ToString());
    //     var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
    //     return descriptionAttribute?.Description ?? value.ToString();
    // }
}