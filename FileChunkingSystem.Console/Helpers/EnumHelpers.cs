using FileChunkingSystem.Console.Extensions;

namespace FileChunkingSystem.Console.Helpers;

public static class EnumHelpers
{
    public static Dictionary<string, TEnum> ToDictionary<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .ToDictionary(e => (e as Enum).GetDescription(), e => e);
    }
}