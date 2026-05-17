using Microsoft.AspNetCore.Mvc;
using TestProject.Api.Contracts;
using TestProject.Api.Files;
using TestProject.Api.Mapping;
using static TestProject.Api.Assert;

namespace TestProject.Api.Controllers;

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

    /* Note: Uploads
     * 
     * I opted for a non-streaming upload, which is significantly simpler to implement and doesn't
     * require quite so much abstraction-breaking with the domain layer. ASP.NET will buffer the
     * full file contents in memory, which for larger files is going to be a problem. If we want
     * larger files, we need to rewrite this to use streaming instead.
     * 
     * I also have not explicitly set payload length limits. The default limits will be used.
     */

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
