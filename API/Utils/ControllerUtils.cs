namespace API.Utils;

public static class ControllerUtils
{
    public static string SanitizeFileNameForHeader(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "arquivo";

        var cleaned = new string(fileName.Where(c => !char.IsControl(c)).ToArray()).Trim();

        cleaned = cleaned.Replace("\"", "'");

        return string.IsNullOrWhiteSpace(cleaned) ? "arquivo" : cleaned;
    }
}
