using System.Diagnostics;

namespace TestProject.Api.Files;

public sealed class MovePath
{
    private readonly ILogger<MovePath> _logger;
    private readonly FolderConfiguration _config;

    public MovePath(ILogger<MovePath> logger, FolderConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Result<bool> Move(string sourcePath, string targetPath)
    {
        var srcResult = PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, sourcePath)
            .Then(loc => loc.Validate());
        if (srcResult.IsError)
            return srcResult.GetErrorOrThrow();

        var destResult = PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, targetPath)
            .Then(loc => loc.Validate());
        if (destResult.IsError)
            return destResult.GetErrorOrThrow();

        var src = srcResult.GetValueOrThrow();
        var dest = destResult.GetValueOrThrow();
        _logger.LogInformation("Request to move {Source} to {Destination}", src.Name, dest.Name);
        return Move(src, dest);
    }

    private Result<bool> Move(PathLocation src, PathLocation dest)
    {
        if (src.IsSameLocationAs(dest))
            return new NothingToDo();

        return (src, dest) switch
        {
            ({ Exists: false }, _) => new LocationDoesNotExist(src),
            (_, { Exists: true, Type: EntryType.Folder }) => MoveIntoExistingFolder(src, dest),
            (_, { Exists: true }) => new WriteConflict(dest),
            (_, { Exists: false }) => MoveDirectly(src, dest)
        };
    }

    private Result<bool> MoveIntoExistingFolder(PathLocation src, PathLocation dest)
    {
        Debug.Assert(dest.Type == EntryType.Folder);
        try
        {
            if (src.Type == EntryType.File)
            {
                File.Move(src.FullPath, Path.Combine(dest.FullPath, src.GetFileInfo().Name), false);
                return true;
            }
            if (src.Type == EntryType.Folder)
            {
                Directory.Move(src.FullPath, Path.Combine(dest.FullPath, new DirectoryInfo(src.FullPath).Name));
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            return new CannotMove(src, dest, ex);
        }
    }

    private Result<bool> MoveDirectly(PathLocation src, PathLocation dest)
    {
        // It is entirely possible that the dest points to a nested folder location which does not exist
        // We are going to lean on the system file operations to detect that case.
        try
        {
            if (src.Type == EntryType.File)
            {
                File.Move(src.FullPath, dest.FullPath, false);
                return true;
            }
            if (src.Type == EntryType.Folder)
            {
                Directory.Move(src.FullPath, dest.FullPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            return new CannotMove(src, dest, ex);
        }
    }
}