namespace TestProject.Api.Files;

public enum EntryType
{
    Unknown,
    File,
    Folder
}

public readonly record struct Entry(
    PathLocation Location,
    EntryType Type,
    long SizeBytes,
    DateTimeOffset CreatedDate,
    DateTimeOffset ModifiedDate);
