using TestProject.Api.Contracts;
using TestProject.Api.Files;

namespace TestProject.Api.Mapping;

public static class ContentsMapper
{
    public static ContentEntriesResponse Map(Contents contents)
        => contents switch
        {
            SearchContents sc => MapSearchContents(sc),
            DirectoryContents dc => MapDirectoryContents(dc),
            _ => new ContentEntriesResponse
            {
                Name = string.Empty,
                Entries = []
            }
        };

    private static ContentEntry MapEntry(Entry entry)
        => new ContentEntry
        {
            Name = entry.Location.Name,
            Size = entry.SizeBytes,
            CreateDate = entry.CreatedDate,
            ModifiedDate = entry.ModifiedDate,
            Type = entry.Type
        };

    private static ContentEntriesResponse MapSearchContents(SearchContents searchResults)
        => new ContentEntriesResponse
        {
            Name = searchResults.Location.Name,
            Entries = searchResults.Entries
                .Select(MapEntry)
                .ToList(),
            FilesCount = searchResults.Entries.Count(e => e.Type == EntryType.File),
            FoldersCount = searchResults.Entries.Count(e => e.Type == EntryType.Folder)
        };

    private static ContentEntriesResponse MapDirectoryContents(DirectoryContents directory)
        => new ContentEntriesResponse
        {
            Name = directory.Location.Name,
            Entries = directory.Entries
                .Select(MapEntry)
                .ToList(),
            FilesCount = directory.Entries.Count(e => e.Type == EntryType.File),
            FoldersCount = directory.Entries.Count(e => e.Type == EntryType.Folder)
        };
}