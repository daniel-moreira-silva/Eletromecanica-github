namespace Core.Utils;

public static class DateTimeUtils
{
    public static string ToBrazilianFormat(this DateTime? dateTime) => dateTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty;

    public static string ToBrazilianFormat(this DateTime dateTime) => dateTime.ToString("dd/MM/yyyy HH:mm:ss");
}
