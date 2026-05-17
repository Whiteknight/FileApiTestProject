namespace TestProject.Tests;

[SetUpFixture]
public class TestFixture
{
    private static int _counter = 1;

    public static string RootDirectory { get; private set; } = string.Empty;

    public static DirectoryInfo RootDirectoryInfo => new DirectoryInfo(RootDirectory);

    public static (string Name, string Path) CreateUniqueDirectory()
    {
        var counter = _counter++;
        var name = counter.ToString();
        var path = Path.Combine(RootDirectory, name);
        Directory.CreateDirectory(path);
        return (name, path);
    }

    [OneTimeSetUp]
    public void CreateTempFolder()
    {
        RootDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(RootDirectory);
    }

    [OneTimeTearDown]
    public void CleanupTempFolder()
    {
        if (Directory.Exists(RootDirectory))
            Directory.Delete(RootDirectory, recursive: true);
    }
}

