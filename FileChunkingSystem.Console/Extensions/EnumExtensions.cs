using System.ComponentModel;
using System.Reflection;

namespace FileChunkingSystem.Console.Extensions;

/// <summary>
/// Provides extension methods for working with enum types, particularly for retrieving description attributes.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description attribute value of an enum value, or returns the enum name if no description is found.
    /// </summary>
    /// <param name="value">The enum value to get the description for</param>
    /// <returns>The description attribute value or the enum name</returns>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
