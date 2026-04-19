namespace Goldfinch.Web.Infrastructure.StaticFiles;

public class StaticFilesCacheOptions
{
    public const string SectionName = "StaticFiles:Cache";

    public bool Enabled { get; set; } = true;

    public int CacheDurationSeconds { get; set; } = 31536000;

    public bool Immutable { get; set; } = true;
}
