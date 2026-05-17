using System.Diagnostics;

namespace TestProject.Api.Files;

public readonly record struct PathLocation(DirectoryInfo Root, string FullPath, string Name, bool Exists, EntryType Type, bool Hidden)
{
    public static PathLocation From(DirectoryInfo root, FileSystemInfo info)
    {
        var relative = Path.GetRelativePath(root.FullName, info.FullName);
        var canonical = relative.Replace('\\', '/').Trim('/');
        if (!info.Exists)
            return new PathLocation(root, info.FullName, canonical, false, EntryType.Unknown, false);

        var attrs = info.Attributes;
        var type = attrs.HasFlag(FileAttributes.Directory)
            ? EntryType.Folder
            : EntryType.File;
        var hidden = attrs.HasFlag(FileAttributes.Hidden) || attrs.HasFlag(FileAttributes.System);
        return new PathLocation(root, info.FullName, canonical, true, type, hidden);
    }

    // Surface-level sanitation on the input path, rejecting invalid characters, rejecting paths which are too long,
    // canonicalize it to use '/' for the Name but platform-specific separators for paths, and returning
    // a PathLocation object ready to use.
    public static Result<PathLocation> SanitizeAndCanonicalize(DirectoryInfo rootDirectory, string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return new PathLocation(rootDirectory, rootDirectory.FullName, string.Empty, true, EntryType.Folder, false);

        var invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) != -1)
            return new PathIsInvalid(path);

        path = path.Trim('/').Trim('\\');
        var canonical = path.Replace('\\', '/').Trim('/');
        var local = Path.DirectorySeparatorChar switch
        {
            '/' => canonical,
            '\\' => path.Replace('/', Path.DirectorySeparatorChar),
            _ => path
        };

        var fullPath = Path.Combine(rootDirectory.FullName, local);
        return new PathLocation(rootDirectory, fullPath, canonical, false, EntryType.Unknown, false);
    }

    // Structural validation. Makes sure that the given path is located within the root directory only.
    public Result<PathLocation> Validate()
    {
        var fullChild = Path.GetFullPath(FullPath);
        var relative = Path.GetRelativePath(Root.FullName, fullChild);
        if (relative.StartsWith("..") || Path.IsPathRooted(relative))
            return new LocationIsOutsideTheRootFolder(Name);

        if (!File.Exists(FullPath) && !Directory.Exists(FullPath))
            return this with { Exists = false, Hidden = false };

        var attrs = File.GetAttributes(FullPath);
        return this with
        {
            Exists = true,
            Type = attrs.HasFlag(FileAttributes.Directory)
                ? EntryType.Folder
                : EntryType.File,
            Hidden = attrs.HasFlag(FileAttributes.Hidden) || attrs.HasFlag(FileAttributes.System)
        };
    }

    public PathLocation Append(string name)
    {
        // Append operation doesn't really make sense if the path is a file, but for now we can put an assert here
        // and just check callsites to make sure we only do this for directories.
        Debug.Assert(Type == EntryType.Folder);
        if (string.IsNullOrEmpty(name))
            return this;

        if (Name == string.Empty)
            return new PathLocation(Root, Path.Combine(FullPath, name), name.Trim('/').Trim('\\'), false, EntryType.Unknown, false);

        return new PathLocation(Root, Path.Combine(FullPath, name), Name + '/' + name.Trim('/').Trim('\\'), false, EntryType.Unknown, false);
    }

    public DirectoryInfo GetDirectoryInfo()
        => new DirectoryInfo(FullPath);

    public FileInfo GetFileInfo()
        => new FileInfo(FullPath);

    public bool IsSameLocationAs(PathLocation other)
        => Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
}
