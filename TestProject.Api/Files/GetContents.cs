using System.Diagnostics;
using Microsoft.AspNetCore.StaticFiles;
using TestProject.Api.Contracts;
using static TestProject.Api.Assert;

namespace TestProject.Api.Files;

public sealed class GetContents
{
    /* Note: Hidden files
     * 
     * On Windows files are marked as hidden or not using metadata. It would be expected that a
     * file like .env or .git would be visible as part of normal directory traversal. However
     * on Linux there is no concept of a hidden file attribute and it would be expected to not
     * Show these files. For the sake of argument, I have followed the Windows Conventions in this
     * API. Files marked as Hidden or System files will not be shown, but dot files will be.
     */

    private readonly ILogger<GetContents> _logger;
    private readonly FolderConfiguration _config;
    private readonly FileExtensionContentTypeProvider _mimeTypes;

    public GetContents(ILogger<GetContents> logger, FolderConfiguration config, FileExtensionContentTypeProvider mimeTypes)
    {
        _logger = NotNull(logger);
        _config = NotNull(config);
        _mimeTypes = NotNull(mimeTypes);
    }

    public Result<Contents> Get(string path)
        => PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, path)
            .Then(loc => loc.Validate())
            .OnSuccess(loc => _logger.LogInformation("Read attempt at {Path} started", loc.FullPath))
            .Then(GetContentsFromPath)
            .OnSuccess(contents => _logger.LogInformation("Read attempt at {Path} success", contents.Location.FullPath));

    public Result<Contents> Search(FileSearchRequest request)
        => PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, request.Path)
            .Then(loc => loc.Validate())
            .OnSuccess(loc => _logger.LogInformation("Search attempt at {Path} with '{Pattern}' started", loc.FullPath, request.Pattern))
            .Then(loc => GetFilePaths(loc, request.Pattern, request.GetPageSize()))
            .Then(x => MapSearchResultsToContents(x.Location, x.Results))
            .OnSuccess(contents => _logger.LogInformation("Search attempt at {Path} with '{Pattern}' success", contents.Location.FullPath, request.Pattern));

    private static string GetGlobPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return "*";
        if (pattern.Contains('*') || pattern.Contains('?'))
            return pattern;
        return $"*{pattern}*";
    }

    private static Result<(PathLocation Location, FileSystemInfo[] Results)> GetFilePaths(PathLocation path, string requestPattern, int pageSize)
    {
        if (!path.Exists)
            return new LocationDoesNotExist(path);

        var pattern = GetGlobPattern(requestPattern);
        try
        {
            var results = path.GetDirectoryInfo()
                .EnumerateFileSystemInfos(pattern, new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple
                })
                .Where(item => (item.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                .Take(pageSize)
                .ToArray();
            return (path, results);
        }
        catch (Exception ex)
        {
            return new CannotSearch(requestPattern, ex);
        }
    }

    private Result<Contents> MapSearchResultsToContents(PathLocation location, FileSystemInfo[] infos)
    {
        var entries = infos
            .Select(item => item switch
            {
                DirectoryInfo dir => new Entry(PathLocation.From(_config.RootDirectory, dir), EntryType.Folder, 0, dir.CreationTimeUtc, dir.LastWriteTimeUtc),
                FileInfo file => new Entry(PathLocation.From(_config.RootDirectory, file), EntryType.File, file.Length, file.CreationTimeUtc, file.LastWriteTimeUtc),
                _ => default
            })
            .ToArray();
        return new SearchContents(location, entries);
    }

    private Result<Contents> GetContentsFromPath(PathLocation location)
        => location switch
        {
            { Exists: false } => new LocationDoesNotExist(location),
            { Hidden: true } => new LocationDoesNotExist(location),
            { Type: EntryType.File } => GetFileContents(location),
            { Type: EntryType.Folder } => GetDirectoryContents(location),
            _ => throw new UnreachableException("Unknown attributes combination")
        };

    private Result<Contents> GetFileContents(PathLocation location)
    {
        try
        {
            var file = location.GetFileInfo();
            var contentType = _mimeTypes.TryGetContentType(location.FullPath, out var ct) ? ct : "application/octet-stream";
            var stream = file.OpenRead();
            return new FileContents(location, file.Name, stream, contentType, file.Length, file.CreationTimeUtc, file.LastWriteTimeUtc);
        }
        catch (Exception ex)
        {
            return new LocationCannotBeRead(location, ex);
        }
    }

    private static Result<Contents> GetDirectoryContents(PathLocation location)
    {
        try
        {
            var directory = location.GetDirectoryInfo();
            var entries = directory.EnumerateFileSystemInfos()
                .Where(item => (item.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                .Select(item => item switch
                {
                    DirectoryInfo dir => new Entry(location.Append(dir.Name), EntryType.Folder, 0, dir.CreationTimeUtc, dir.LastWriteTimeUtc),
                    FileInfo file => new Entry(location.Append(file.Name), EntryType.File, file.Length, file.CreationTimeUtc, file.LastWriteTimeUtc),
                    _ => default
                })
                .Where(entry => entry.Type != EntryType.Unknown)
                .ToList();

            return new DirectoryContents(location, entries);
        }
        catch (Exception ex)
        {
            return new LocationCannotBeRead(location, ex);
        }
    }
}
