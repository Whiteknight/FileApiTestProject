using System.Net.Http.Json;
using Reqnroll;
using TestProject.Api.Contracts;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class BrowseSteps
{
    private readonly ScenarioContext _context;

    public BrowseSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpClient Client => _context.Get<HttpClient>();

    [When(@"I browse the root directory")]
    public async Task WhenIBrowseTheRootDirectory()
    {
        var response = await Client.GetAsync("/Files?path=/");
        _context.Set(response, "Response");
        if (response.IsSuccessStatusCode)
            _context.Set(await response.Content.ReadFromJsonAsync<ContentEntriesResponse>(), "ResponseBody");
    }

    [When(@"I browse the path ""(.+)""")]
    public async Task WhenIBrowseThePath(string path)
    {
        var response = await Client.GetAsync($"/Files?path={Uri.EscapeDataString(path)}");
        _context.Set(response, "Response");
        if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/json")
            _context.Set(await response.Content.ReadFromJsonAsync<ContentEntriesResponse>(), "ResponseBody");
    }
}
