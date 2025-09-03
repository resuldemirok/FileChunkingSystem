using FileChunkingSystem.Console.Extensions;

namespace FileChunkingSystem.Console.Helpers;

/// <summary>
/// Provides helper methods for working with enum types in console applications.
/// </summary>
public static class EnumHelpers
{
    /// <summary>
    /// Converts an enum type to a dictionary where keys are descriptions and values are enum values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert</typeparam>
    /// <returns>A dictionary mapping descriptions to enum values</returns>
    public static Dictionary<string, TEnum> ToDictionary<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .ToDictionary(e => (e as Enum).GetDescription(), e => e);
    }
}