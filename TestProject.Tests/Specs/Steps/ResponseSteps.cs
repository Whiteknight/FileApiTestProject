using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Reqnroll;
using TestProject.Api.Contracts;
using TestProject.Api.Files;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class ResponseSteps
{
    private readonly ScenarioContext _context;

    public ResponseSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpResponseMessage Response => _context.Get<HttpResponseMessage>("Response");
    private ContentEntriesResponse? Body => _context.TryGetValue<ContentEntriesResponse>("ResponseBody", out var b) ? b : null;

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int statusCode)
    {
        Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"the response contains (\d+) entries")]
    public void ThenTheResponseContainsEntries(int count)
    {
        Body.Should().NotBeNull();
        Body!.Entries.Should().HaveCount(count);
    }

    [Then(@"the response contains an entry ""(.+)"" of type ""(File|Folder)""")]
    public void ThenTheResponseContainsAnEntryOfType(string name, string type)
    {
        Body.Should().NotBeNull();
        var expectedType = Enum.Parse<EntryType>(type);
        Body!.Entries.Should().Contain(e => e.Name == name && e.Type == expectedType);
    }

    [Then(@"the response contains the following entries:")]
    public void ThenTheResponseContainsTheFollowingEntries(Table table)
    {
        Body.Should().NotBeNull();
        foreach (var row in table.Rows)
        {
            var name = row["Path"];
            var expectedType = Enum.Parse<EntryType>(row["Type"]);
            Body!.Entries.Should().Contain(e => e.Name == name && e.Type == expectedType);
        }
    }

    [Then(@"the response content type starts with ""(.+)""")]
    public void ThenTheResponseContentTypeStartsWith(string expected)
    {
        Response.Content.Headers.ContentType?.MediaType.Should().StartWith(expected);
    }

    [Then(@"the response body is ""(.+)""")]
    public async Task ThenTheResponseBodyIs(string expected)
    {
        var content = await Response.Content.ReadAsStringAsync();
        content.Should().Be(expected);
    }
}
