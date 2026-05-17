using Reqnroll;

namespace TestProject.Tests.Specs.Support;

[Binding]
public sealed class ScenarioHooks
{
    private readonly ScenarioContext _context;

    public ScenarioHooks(ScenarioContext context)
    {
        _context = context;
    }

    [BeforeScenario]
    public void CreateScenarioDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "specs", Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        var root = new DirectoryInfo(path);

        var factory = new TestApiFactory { ScenarioRoot = root };
        _context.Set(root);
        _context.Set(factory);
        _context.Set(factory.CreateClient());
    }

    [AfterScenario]
    public void CleanupScenarioDirectory()
    {
        if (_context.TryGetValue<TestApiFactory>(out var factory))
            factory.Dispose();

        if (_context.TryGetValue<DirectoryInfo>(out var root) && root.Exists)
            root.Delete(recursive: true);
    }
}
