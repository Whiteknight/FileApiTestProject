using System.Net.Http.Json;
using Reqnroll;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class MoveSteps
{
    private readonly ScenarioContext _context;

    public MoveSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpClient Client => _context.Get<HttpClient>();

    [When(@"I move ""(.+)"" to ""(.+)""")]
    public async Task WhenIMoveTo(string source, string destination)
    {
        var response = await Client.PostAsJsonAsync("/Files/move", new { sourcePath = source, destinationPath = destination });
        _context.Set(response, "Response");
    }
}
