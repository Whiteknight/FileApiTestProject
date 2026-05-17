using TestProject.Api.Files;

namespace TestProject.Api.Contracts;

public class FileSearchRequest
{
    public int PageSize { get; set; }

    public required string Path { get; set; }

    public required string Pattern { get; set; }

    public int GetPageSize()
        => PageSize <= 0 || PageSize > 500 ? 100 : PageSize;
}


public sealed class ContentEntry
{
    public required string Name { get; set; }
    public long Size { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public EntryType Type { get; set; }
}

public class ContentEntriesResponse
{
    public required string Name { get; set; }

    public required List<ContentEntry> Entries { get; set; }

    public int FilesCount { get; set; }

    public int FoldersCount { get; set; }
}
