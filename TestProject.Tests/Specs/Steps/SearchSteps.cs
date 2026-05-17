using System.Net.Http.Json;
using Reqnroll;
using TestProject.Api.Contracts;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class SearchSteps
{
    private readonly ScenarioContext _context;

    public SearchSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpClient Client => _context.Get<HttpClient>();

    [When(@"I search for ""(.+)"" in path ""(.+)""")]
    public async Task WhenISearchInPath(string pattern, string path)
    {
        var url = $"/Files/search?path={Uri.EscapeDataString(path)}&pattern={Uri.EscapeDataString(pattern)}";
        var response = await Client.GetAsync(url);
        _context.Set(response, "Response");
        if (response.IsSuccessStatusCode)
            _context.Set(await response.Content.ReadFromJsonAsync<ContentEntriesResponse>(), "ResponseBody");
    }

    [When(@"I search for ""(.+)"" in path ""(.+)"" with page size (\d+)")]
    public async Task WhenISearchInPathWithPageSize(string pattern, string path, int pageSize)
    {
        var url = $"/Files/search?path={Uri.EscapeDataString(path)}&pattern={Uri.EscapeDataString(pattern)}&pageSize={pageSize}";
        var response = await Client.GetAsync(url);
        _context.Set(response, "Response");
        if (response.IsSuccessStatusCode)
            _context.Set(await response.Content.ReadFromJsonAsync<ContentEntriesResponse>(), "ResponseBody");
    }
}
