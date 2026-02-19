namespace Goldfinch.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts application-relative paths (~/path) to absolute paths (/path).
    /// Returns the original path if it doesn't start with ~/.
    /// </summary>
    public static string ToAbsolutePath(this string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path ?? string.Empty;
        }

        return path.StartsWith("~/") ? path.Substring(1) : path;
    }
}
