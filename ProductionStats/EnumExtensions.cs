using System.ComponentModel;
using System.Reflection;

namespace ProductionStats;

internal static class EnumExtensions
{
    /// <summary>
    /// Get the description for a given value of the enum.
    /// </summary>
    /// <param name="value">Value of the enum</param>
    /// <returns>
    ///     Description of the given enum. If description is not defined
    ///     returns empty string.
    /// </returns>
    public static string GetDescription(this Enum value)
    {
        Type? type = value.GetType();

        if (type == null)
            return string.Empty;

        string? name = Enum.GetName(type, value);
        if (name == null)
        {
            return string.Empty;
        }

        FieldInfo? field = type.GetField(name);
        if (field == null)
        {
            return string.Empty;
        }

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
        {
            return attr.Description;
        }

        return string.Empty;
    }
}
