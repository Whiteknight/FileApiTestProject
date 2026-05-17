using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TestProject.Api;
using TestProject.Api.Contracts;
using TestProject.Api.Files;
using static TestProject.Api.Assert;

namespace TestProject.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;

    public FilesController(ILogger<FilesController> logger)
    {
        _logger = NotNull(logger);
    }

    [HttpGet]
    public IActionResult Get(
        [FromServices] GetContents get,
        [FromQuery] string path)
        => get.Get(path)
            .OnError(LogErrors)
            .Match(
                contents => contents switch
                {
                    DirectoryContents dir => Ok(ContentsMapper.Map(dir)),
                    FileContents file => file.MapToFileDownload(),
                    _ => BadRequest()
                },
                err => err.MapToHttpResponse());

    [HttpGet("search")]
    public IActionResult Search(
        [FromServices] GetContents search,
        [FromQuery] FileSearchRequest request)
        => search.Search(request)
            .OnError(LogErrors)
            .Match(
                results => Ok(ContentsMapper.Map(results)),
                err => err.MapToHttpResponse());

    [HttpPost("move")]
    public IActionResult Move(
        [FromServices] MovePath move,
        [FromBody] FileMoveRequest request)
        => move.Move(request.SourcePath, request.DestinationPath)
            .OnError(LogErrors)
            .Match(
                result => Ok(),
                err => err.MapToHttpResponse());

    [HttpDelete]
    public IActionResult Delete(
        [FromServices] DeletePath delete,
        [FromBody] FileRequest request)
        => delete.Delete(request.Path)
            .OnError(LogErrors)
            .Match(
                _ => Ok(),
                err => err.MapToHttpResponse());

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        [FromServices] UploadFile upload,
        IFormFile file,
        [FromForm] string targetDirectory,
        CancellationToken cancellation)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var name = Path.Combine(targetDirectory, Path.GetFileName(file.FileName));
        var result = await upload.Upload(name, file.CopyToAsync, cancellation);
        return result
            .OnError(LogErrors)
            .Match(
                _ => Ok(),
                err => err.MapToHttpResponse());
    }

    private void LogErrors(Error error)
    {
        if (error.Exception is { } ex)
            _logger.LogError(ex, "Error with user request {Error}", error.Message);
        else
            _logger.LogError("Error with user request {Error}", error.Message);
    }
}

public static class ResponseMapping
{
    public static FileStreamResult MapToFileDownload(this FileContents file)
        => new FileStreamResult(file.Stream, file.ContentType)
        {
            FileDownloadName = file.GetDownloadName(),
            LastModified = file.ModifyDate,
            EntityTag = new EntityTagHeaderValue($"\"{file.CreateUniqueTag}\""),
            EnableRangeProcessing = true
        };

    public static IActionResult MapToHttpResponse(this Error error)
        => error switch
        {
            NothingToDo => new NoContentResult(),
            PathIsInvalid => new BadRequestResult(),
            LocationDoesNotExist => new NotFoundResult(),

            // 403 Forbidden is a bit weird in an unauthenticated API, but it makes sense when the user is attempting
            // to craft a request which is outside our bounds, which feels malicious.
            // Make clear: We see what you're attempting, stop it.
            // Note: We use StatusCodeResult instead of ForbidResult because ForbidResult requires an authentication scheme.
            LocationIsOutsideTheRootFolder => new StatusCodeResult(StatusCodes.Status403Forbidden),

            // Permissions with respect to the server principal. If the server cannot read the file, that implies something at the system level,
            // not something with the API user, which means we can treat the file as if it does not exist. We don't want to throw a 403 Forbidden
            // and give away the fact that certain system files might exist.
            LocationCannotBeRead => new NotFoundResult(),

            WriteConflict => new ConflictResult(),

            // For other cases not explicitly mapped, return a 500 and hope we get enough info in logging to fix.
            null => new ObjectResult(new ProblemDetails
            {
                Detail = "Unknown internal server error",
                Status = 500,
                Title = "Unknown internal server error"
            }),
            _ => new ObjectResult(new ProblemDetails
            {
                Detail = error.Message,
                Status = 500,
                Title = "Unknown internal server error"
            })
        };
}

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