using AwesomeAssertions;
using Reqnroll;

namespace TestProject.Tests.Specs.Support;

[Binding]
public sealed class FileSystemSteps
{
    private readonly ScenarioContext _context;

    public FileSystemSteps(ScenarioContext context)
    {
        _context = context;
    }

    private DirectoryInfo Root => _context.Get<DirectoryInfo>();

    [Given(@"the following folders exist:")]
    public void GivenTheFoldersExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            var path = Path.Combine(Root.FullName, row["Path"].Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(path);
        }
    }

    [Given(@"the following files exist:")]
    public void GivenTheFilesExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            var relativePath = row["Path"].Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(Root.FullName, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            var content = row.TryGetValue("Content", out var c) ? c : "dummy";
            File.WriteAllText(fullPath, content);
        }
    }

    [Given(@"a folder ""(.+)"" exists")]
    public void GivenAFolderExists(string path)
    {
        Directory.CreateDirectory(Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    [Given(@"a file ""(.+)"" exists")]
    public void GivenAFileExists(string path)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "dummy");
    }

    [Given(@"a file ""(.+)"" exists with content ""(.+)""")]
    public void GivenAFileExistsWithContent(string path, string content)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    [Then(@"the file ""(.+)"" exists on disk")]
    public void ThenTheFileExistsOnDisk(string path)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(fullPath).Should().BeTrue($"expected file '{path}' to exist");
    }

    [Then(@"the file ""(.+)"" exists on disk with content ""(.+)""")]
    public void ThenTheFileExistsOnDiskWithContent(string path, string content)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(fullPath).Should().BeTrue($"expected file '{path}' to exist");
        File.ReadAllText(fullPath).Should().Be(content);
    }

    [Then(@"the file ""(.+)"" does not exist on disk")]
    public void ThenTheFileDoesNotExistOnDisk(string path)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(fullPath).Should().BeFalse($"expected file '{path}' to not exist");
    }

    [Then(@"the folder ""(.+)"" does not exist on disk")]
    public void ThenTheFolderDoesNotExistOnDisk(string path)
    {
        var fullPath = Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.Exists(fullPath).Should().BeFalse($"expected folder '{path}' to not exist");
    }
}
