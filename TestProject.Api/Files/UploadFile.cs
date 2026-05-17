namespace TestProject.Api.Files;

public sealed class UploadFile
{
    private readonly ILogger<UploadFile> _logger;
    private readonly FolderConfiguration _config;

    public UploadFile(ILogger<UploadFile> logger, FolderConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<Result<bool>> Upload(string path, Func<Stream, CancellationToken, Task> writeToStream, CancellationToken cancellation)
    {
        var destResult = PathLocation.SanitizeAndCanonicalize(_config.RootDirectory, path)
            .Then(loc => loc.Validate())
            .OnSuccess(loc => _logger.LogInformation("Upload request to {Path} started", loc.FullPath));
        if (destResult.IsError)
            return destResult.GetErrorOrThrow();
        var dest = destResult.GetValueOrThrow(); ;

        try
        {
            using var stream = dest.GetFileInfo().Open(FileMode.Create, FileAccess.Write, FileShare.None);
            await writeToStream(stream, cancellation);
            return true;
        }
        catch (Exception ex)
        {
            return new CannotWrite(dest, ex);
        }
    }
}
