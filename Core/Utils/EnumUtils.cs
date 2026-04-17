namespace Core.Utils;

public static class EnumUtils
{
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name is null) return value.ToString();

        var field = type.GetField(name);
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();

        return attr?.Description ?? name;
    }
}
