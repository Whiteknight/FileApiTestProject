namespace TestProject.Api.Files;

public abstract record Contents(PathLocation Location);

public sealed record DirectoryContents(PathLocation Location, IReadOnlyList<Entry> Entries)
    : Contents(Location);

public sealed record FileContents(PathLocation Location, string FileName, FileStream Stream, string ContentType, long Size, DateTimeOffset CreateDate, DateTimeOffset ModifyDate)
    : Contents(Location)
{
    public string CreateUniqueTag()
        => $"{Convert.ToString(ModifyDate.Ticks, 16)}_{Convert.ToString(Size, 16)}";

    public string? GetDownloadName()
        => CanDisplayInline()
            ? null
            : FileName;

    private bool CanDisplayInline()
    {
        if (string.IsNullOrWhiteSpace(ContentType))
            return false;

        // Images except some custom formats can display in-line.
        // TODO: a configurable list of specific image types would be nice
        if (ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return !ContentType.Contains("psd") && !ContentType.Contains("tiff");

        // TODO: Some kind of browser capability detection would be better.
        return ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
            ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record SearchContents(PathLocation Location, IReadOnlyList<Entry> Entries)
    : Contents(Location);