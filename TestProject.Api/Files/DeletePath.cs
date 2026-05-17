namespace TestProject.Api.Files;

using static TestProject.Api.Assert;

// Deleted=false means there was nothing to delete. Deleted=true means we deleted the thing.
public readonly record struct DeleteResult(PathLocation Location, bool Deleted);

public sealed class DeletePath
{
    private readonly ILogger<DeletePath> _logger;
    private readonly FolderConfiguration _config;

    public DeletePath(ILogger<DeletePath> logger, FolderConfiguration config)
    {
        _logger = NotNull(logger);
        _config = NotNull(config);
    }

    public Result<DeleteResult> Delete(string path)
        => PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, path)
            .Then(loc => loc.Validate())
            .OnSuccess(loc => _logger.LogInformation("Delete attempt at {Path} started", loc.FullPath))
            .Then(Delete)
            .OnSuccess(result => _logger.LogInformation("Delete attempt at {Path} completed: {Done}", result.Location, result.Deleted));

    private Result<DeleteResult> Delete(PathLocation location)
    {
        if (!location.Exists || location.Hidden)
            return new DeleteResult(location, false);

        try
        {
            if (location.Type == EntryType.File)
            {
                File.Delete(location.FullPath);
                return new DeleteResult(location, true);
            }
            if (location.Type == EntryType.Folder)
            {
                Directory.Delete(location.FullPath, true);
                return new DeleteResult(location, true);
            }
            return new DeleteResult(location, false);
        }
        catch (Exception ex)
        {
            return new CannotDelete(location, ex);
        }
    }
}
