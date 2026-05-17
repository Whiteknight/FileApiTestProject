namespace TestProject.Api.Files;

/* Note: Error types
 * 
 * There are a lot of ways we could slice and dice up the error scenarios into different objects.
 * The key considerations are: 
 * 1. What we want to include in the logs
 * 2. What messaging we want to send to the user, and
 * 3. The HTTP status codes that we want to return
 * Considering that the error messages we show the user are relatively thin and information-sparse,
 * and we are deliberately mapping to a smaller subset of status codes to avoid exposing
 * unnecessary information about hidden/system files, I think we can get away with this small set
 * of errors here.
 */

public sealed record PathIsInvalid(string Path, Exception? Exception = null)
    : Error($"The path '{Path}' contains invalid characters, is too long, or is in an invalid format", Exception);

public sealed record LocationDoesNotExist(PathLocation Location)
    : Error($"Specified folder '{Location.Name}' or the root directory is invalid, missing, empty, or the application does not have access.");

public sealed record LocationIsOutsideTheRootFolder(string Name)
    : Error($"The provided path '{Name}' points to an area outside the folder server");

public sealed record LocationCannotBeRead(PathLocation Location, Exception? Exception)
    : Error($"The location '{Location.Name}' cannot be read due to security or functional issues", Exception);

// This should (probably) become a 500 because a search failure most likely indicates some kind of 
// IO failure and not a bad request issue.
public sealed record CannotSearch(string Pattern, Exception Exception)
    : Error($"Cannot search with the provided pattern '{Pattern}'. It may contain invalid characters or there may be a device failure", Exception);

public sealed record CannotDelete(PathLocation Location, Exception Exception)
    : Error($"Cannot delete item at '{Location.Name}'", Exception);

public sealed record CannotWrite(PathLocation Location, Exception Exception)
    : Error($"Cannot write to location '{Location.Name}'", Exception);

public sealed record WriteConflict(PathLocation Location)
    : Error($"Cannot upload or move file, the target location {Location.Name} already exists");

public sealed record CannotMove(PathLocation Source, PathLocation Destination, Exception Exception)
    : Error($"Cannot move {Source.Name} to {Destination.Name}", Exception);