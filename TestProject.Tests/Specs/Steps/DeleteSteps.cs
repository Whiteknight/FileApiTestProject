using System.Net.Http;
using System.Net.Http.Json;
using Reqnroll;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class DeleteSteps
{
    private readonly ScenarioContext _context;

    public DeleteSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpClient Client => _context.Get<HttpClient>();

    [When(@"I delete ""(.+)""")]
    public async Task WhenIDelete(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/Files")
        {
            Content = JsonContent.Create(new { path })
        };
        var response = await Client.SendAsync(request);
        _context.Set(response, "Response");
    }
}
