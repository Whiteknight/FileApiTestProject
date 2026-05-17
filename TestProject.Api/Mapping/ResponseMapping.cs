using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TestProject.Api.Files;

namespace TestProject.Api.Mapping;

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

            // 403 Forbidden is a bit weird in an unauthenticated API, but it makes sense when the
            // user is attempting to craft a request which is outside our bounds, which feels
            // malicious.
            // Make clear: We see what you're attempting, stop it.
            // Note: We use StatusCodeResult instead of ForbidResult because ForbidResult requires
            // an authentication scheme.
            LocationIsOutsideTheRootFolder => new StatusCodeResult(StatusCodes.Status403Forbidden),

            // Permissions with respect to the server principal. If the server cannot read the
            // file, it implies something is wrong at the system level not something with the API
            // user. This means we can treat the file as if it does not exist. We don't want to
            // throw a 403 Forbidden and give away the fact that certain system files might exist.
            LocationCannotBeRead => new NotFoundResult(),

            WriteConflict => new ConflictResult(),

            // For other cases not explicitly mapped, return a 500. Hopefully we see these cases
            // in the logs and can correctly map them in the future.
            _ => new ObjectResult(new ProblemDetails
            {
                Detail = error?.Message ?? "Unknown internal server error",
                Status = 500,
                Title = "Unknown internal server error"
            })
        };
}
